using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;

namespace CommunityVoting.API.Endpoints;

public static class MeetingEndpoints
{
    public static void MapMeetingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/meetings");

        group.MapGet("/", async (IMeetingService meetingService) =>
        {
            var meetings = await meetingService.GetAllMeetingsAsync();
            return Results.Ok(meetings);
        }).RequireAuthorization();

        group.MapPost("/", async (CreateMeetingRequest request, IMeetingService meetingService) =>
        {
            var meeting = await meetingService.CreateMeetingAsync(request);
            if (meeting == null) return Results.BadRequest("No se pudo crear la reunión. Verifique el ID de la comunidad.");
            return Results.Created($"/api/meetings/{meeting.Id}", meeting);
        }).RequireAuthorization();

        group.MapGet("/community/{communityId}", async (Guid communityId, IMeetingService meetingService) =>
        {
            var meetings = await meetingService.GetMeetingsByCommunityAsync(communityId);
            return Results.Ok(meetings);
        }).RequireAuthorization();

        group.MapGet("/{id}", async (Guid id, IMeetingService meetingService) =>
        {
            var meeting = await meetingService.GetMeetingDetailsAsync(id);
            if (meeting == null) return Results.NotFound();
            return Results.Ok(meeting);
        }).RequireAuthorization();

        // Used by Voting.Api service to fetch voting-eligible voters and quorum data
        group.MapGet("/{id}/voting-eligible-data", async (Guid id, IMeetingService meetingService) =>
        {
            var eligibleData = await meetingService.GetVotingEligibleDataAsync(id);
            if (eligibleData == null) return Results.NotFound("Reunión no encontrada.");
            return Results.Ok(eligibleData);
        });

        group.MapGet("/{id}/quorum-status", async (Guid id, IMeetingService meetingService) =>
        {
            var status = await meetingService.GetQuorumStatusAsync(id);
            if (status == null) return Results.NotFound("Reunión no encontrada.");
            return Results.Ok(status);
        });

        group.MapPost("/{id}/attendance", async (Guid id, RecordAttendanceRequest request, IMeetingService meetingService) =>
        {
            var participant = await meetingService.RecordAttendanceAsync(id, request);
            if (participant == null) return Results.NotFound("Reunión no encontrada.");
            return Results.Ok(participant);
        }).RequireAuthorization();

        group.MapGet("/{id}/voting-settings", async (Guid id, IMeetingService meetingService) =>
        {
            var settings = await meetingService.GetVotingSettingsAsync(id);
            if (settings == null) return Results.NotFound("Reunión no encontrada.");
            return Results.Ok(settings);
        }).RequireAuthorization();

        group.MapPut("/{id}/voting-settings", async (Guid id, UpdateVotingSettingsRequest request, IMeetingService meetingService) =>
        {
            var updated = await meetingService.UpdateVotingSettingsAsync(id, request);
            if (updated == null) return Results.NotFound("Reunión no encontrada.");
            return Results.Ok(updated);
        }).RequireAuthorization();

        group.MapPut("/{id}", async (Guid id, UpdateMeetingRequest request, IMeetingService meetingService) =>
        {
            var updated = await meetingService.UpdateMeetingAsync(id, request);
            if (updated == null) return Results.NotFound("Reunión no encontrada.");
            return Results.Ok(updated);
        }).RequireAuthorization();

        group.MapGet("/{id}/participants", async (Guid id, IMeetingService meetingService) =>
        {
            var participants = await meetingService.GetParticipantsAsync(id);
            return Results.Ok(participants);
        }).RequireAuthorization();

        group.MapDelete("/{id}", async (Guid id, IMeetingService meetingService) =>
        {
            var success = await meetingService.DeleteMeetingAsync(id);
            if (!success) return Results.NotFound();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
