using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Services;

namespace CommunityVoting.API.Endpoints;

public static class ProposalEndpoints
{
    public static void MapProposalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/proposals").RequireAuthorization();

        group.MapPost("/", async (CreateProposalRequest request, ProposalService proposalService) =>
        {
            var proposal = await proposalService.CreateProposalAsync(request);
            if (proposal == null) return Results.BadRequest("Reunión no encontrada.");
            return Results.Created($"/api/proposals/{proposal.Id}", proposal);
        });

        group.MapGet("/{id}", async (Guid id, ProposalService proposalService) =>
        {
            var proposal = await proposalService.GetProposalByIdAsync(id);
            if (proposal == null) return Results.NotFound();
            return Results.Ok(proposal);
        });

        group.MapPut("/{id}/majority-settings", async (Guid id, UpdateProposalMajorityRequest request, ProposalService proposalService) =>
        {
            var updated = await proposalService.UpdateMajoritySettingsAsync(id, request);
            if (updated == null) return Results.NotFound("Propuesta no encontrada.");
            return Results.Ok(updated);
        });

        group.MapPost("/{id}/options", async (Guid id, CreateProposalOptionRequest request, ProposalService proposalService) =>
        {
            var req = request with { ProposalId = id };
            var option = await proposalService.AddOptionAsync(req);
            if (option == null) return Results.NotFound();
            return Results.Ok(option);
        });

        group.MapDelete("/options/{optionId}", async (Guid optionId, ProposalService proposalService) =>
        {
            await proposalService.DeleteOptionAsync(optionId);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (Guid id, ProposalService proposalService) =>
        {
            var success = await proposalService.DeleteProposalAsync(id);
            if (!success) return Results.NotFound();
            return Results.NoContent();
        });
    }
}
