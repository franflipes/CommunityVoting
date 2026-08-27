using System.Security.Claims;
using CommunityVoting.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CommunityVoting.Document.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents").RequireAuthorization();

        group.MapPost("/proposal/{proposalId}", async (
            Guid proposalId,
            HttpContext httpContext,
            IDocumentService documentService) =>
        {
            if (!httpContext.Request.HasFormContentType)
            {
                return Results.BadRequest("La petición debe ser multipart/form-data.");
            }

            var form = await httpContext.Request.ReadFormAsync();
            var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
            var title = form["title"].ToString();
            var description = form["description"].ToString();

            if (file == null || file.Length == 0) return Results.BadRequest("El archivo es obligatorio.");
            if (string.IsNullOrWhiteSpace(title)) return Results.BadRequest("El título del documento es obligatorio.");

            var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            using var stream = file.OpenReadStream();
            var doc = await documentService.UploadDocumentAsync(
                proposalId,
                title,
                description ?? string.Empty,
                stream,
                file.FileName,
                file.ContentType,
                userId
            );

            if (doc == null) return Results.BadRequest("Propuesta o usuario no válido.");
            return Results.Created($"/api/documents/{doc.Id}", doc);
        }).DisableAntiforgery();

        group.MapGet("/proposal/{proposalId}", async (Guid proposalId, IDocumentService documentService) =>
        {
            var docs = await documentService.GetDocumentsByProposalAsync(proposalId);
            return Results.Ok(docs);
        });

        group.MapGet("/{id}/download", async (Guid id, IDocumentService documentService) =>
        {
            var result = await documentService.DownloadDocumentAsync(id);
            if (result == null) return Results.NotFound();

            return Results.File(result.Value.stream, result.Value.contentType, result.Value.fileName);
        });

        group.MapDelete("/{id}", async (Guid id, IDocumentService documentService) =>
        {
            var success = await documentService.DeleteDocumentAsync(id);
            if (!success) return Results.NotFound();
            return Results.NoContent();
        });
    }
}
