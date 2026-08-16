using System.Security.Claims;
using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Enums;

namespace CommunityVoting.API.Endpoints;

public static class CommunityEndpoints
{
    public static void MapCommunityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/communities").RequireAuthorization();

        group.MapPost("/", async (CreateCommunityRequest request, HttpContext httpContext, CommunityService communityService) =>
        {
            var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var community = await communityService.CreateCommunityAsync(request, userId);
            return Results.Created($"/api/communities/{community.Id}", community);
        });

        group.MapGet("/", async (HttpContext httpContext, CommunityService communityService) =>
        {
            var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var roleStr = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var role = Enum.TryParse<UserRole>(roleStr, out var parsedRole) ? parsedRole : UserRole.CommunityMember;

            var communities = await communityService.GetCommunitiesForUserAsync(userId, role);
            return Results.Ok(communities);
        });

        group.MapGet("/{id}", async (Guid id, CommunityService communityService) =>
        {
            var community = await communityService.GetCommunityByIdAsync(id);
            if (community == null) return Results.NotFound();
            return Results.Ok(community);
        });

        group.MapPost("/{id}/members", async (Guid id, AddCommunityMemberRequest request, CommunityService communityService) =>
        {
            var member = await communityService.AddMemberAsync(id, request);
            if (member == null) return Results.BadRequest("Comunidad o usuario no encontrado.");
            return Results.Ok(member);
        });

        group.MapGet("/{id}/members", async (Guid id, CommunityService communityService) =>
        {
            var members = await communityService.GetMembersAsync(id);
            return Results.Ok(members);
        });

        group.MapGet("/{id}/voting-settings", async (Guid id, CommunityService communityService) =>
        {
            var settings = await communityService.GetVotingSettingsAsync(id);
            if (settings == null) return Results.NotFound("Comunidad no encontrada.");
            return Results.Ok(settings);
        });

        group.MapPut("/{id}/voting-settings", async (Guid id, UpdateVotingSettingsRequest request, CommunityService communityService) =>
        {
            var updated = await communityService.UpdateVotingSettingsAsync(id, request);
            if (updated == null) return Results.NotFound("Comunidad no encontrada.");
            return Results.Ok(updated);
        });

        group.MapPut("/{id}/members/{memberId}/voting-rights", async (Guid id, Guid memberId, UpdateMemberVotingRightsRequest request, CommunityService communityService) =>
        {
            var updated = await communityService.UpdateMemberVotingRightsAsync(id, memberId, request);
            if (updated == null) return Results.NotFound("Miembro no encontrado.");
            return Results.Ok(updated);
        });
    }
}
