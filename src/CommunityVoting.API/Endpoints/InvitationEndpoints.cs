using System.Security.Claims;
using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;

namespace CommunityVoting.API.Endpoints;

public static class InvitationEndpoints
{
    public static void MapInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invitations");

        // Public token verification
        group.MapGet("/verify/{token}", async (string token, IInvitationService invitationService) =>
        {
            var result = await invitationService.VerifyInvitationAsync(token);
            return Results.Ok(result);
        });

        // Admin: Generate invitation link for a community
        group.MapPost("/community/{communityId}", async (Guid communityId, CreateInvitationRequest request, HttpContext httpContext, IInvitationService invitationService) =>
        {
            var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var req = request with { CommunityId = communityId };
            var origin = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            var invitation = await invitationService.CreateInvitationAsync(req, userId, origin);
            if (invitation == null) return Results.NotFound("Comunidad no encontrada.");
            return Results.Created($"/api/invitations/{invitation.Id}", invitation);
        }).RequireAuthorization();

        // Admin: Get all invitations for a community
        group.MapGet("/community/{communityId}", async (Guid communityId, HttpContext httpContext, IInvitationService invitationService) =>
        {
            var origin = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            var invitations = await invitationService.GetInvitationsByCommunityAsync(communityId, origin);
            return Results.Ok(invitations);
        }).RequireAuthorization();

        // Public: Register using invitation link
        app.MapPost("/api/auth/register-with-invitation", async (RegisterWithInvitationRequest request, IInvitationService invitationService) =>
        {
            var response = await invitationService.RegisterWithInvitationAsync(request);
            if (response == null) return Results.BadRequest(new { error = "El enlace de invitación no es válido o ha expirado." });
            return Results.Ok(response);
        });
    }
}
