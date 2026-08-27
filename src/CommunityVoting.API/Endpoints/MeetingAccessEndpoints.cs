using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;

namespace CommunityVoting.API.Endpoints;

public static class MeetingAccessEndpoints
{
    public static void MapMeetingAccessEndpoints(this IEndpointRouteBuilder app)
    {
        // Public Meeting Access Authentication Endpoint
        app.MapPost("/api/auth/meeting-access", async (MeetingAccessLoginRequest request, IMeetingAccessService meetingAccessService) =>
        {
            try
            {
                var response = await meetingAccessService.AuthenticateAsync(request);
                if (response == null) return Results.BadRequest(new { error = "No se pudo autenticar con el enlace y código proporcionados." });
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // Meeting Voter Access Management Group
        var group = app.MapGroup("/api/meetings/{meetingId}/voter-accesses").RequireAuthorization();

        group.MapGet("/", async (Guid meetingId, HttpContext httpContext, IMeetingAccessService meetingAccessService) =>
        {
            var baseUrl = GetBaseUrl(httpContext);
            var accesses = await meetingAccessService.GetVoterAccessesForMeetingAsync(meetingId, baseUrl);
            return Results.Ok(accesses);
        });

        group.MapPost("/generate-all", async (Guid meetingId, HttpContext httpContext, IMeetingAccessService meetingAccessService) =>
        {
            var baseUrl = GetBaseUrl(httpContext);
            var credentials = await meetingAccessService.CreateAccessForEligibleMembersAsync(meetingId, baseUrl);
            return Results.Ok(credentials);
        });

        group.MapPost("/revoke/{userId}", async (Guid meetingId, Guid userId, IMeetingAccessService meetingAccessService) =>
        {
            var success = await meetingAccessService.RevokeAccessAsync(meetingId, userId);
            if (!success) return Results.NotFound(new { error = "No se encontró el acceso especificado." });
            return Results.Ok(new { message = "Acceso revocado correctamente." });
        });

        group.MapPost("/regenerate/{userId}", async (Guid meetingId, Guid userId, HttpContext httpContext, IMeetingAccessService meetingAccessService) =>
        {
            var baseUrl = GetBaseUrl(httpContext);
            var credential = await meetingAccessService.RegenerateAccessAsync(meetingId, userId, baseUrl);
            if (credential == null) return Results.NotFound(new { error = "No se pudo regenerar el acceso." });
            return Results.Ok(credential);
        });

        group.MapGet("/invitations/statuses", async (Guid meetingId, IMeetingAccessService meetingAccessService) =>
        {
            var statuses = await meetingAccessService.GetInvitationStatusesAsync(meetingId);
            return Results.Ok(statuses);
        });

        group.MapPost("/invitations/resend/{userId}", async (Guid meetingId, Guid userId, HttpContext httpContext, IMeetingAccessService meetingAccessService) =>
        {
            var baseUrl = GetBaseUrl(httpContext);
            var success = await meetingAccessService.ResendInvitationAsync(meetingId, userId, baseUrl);
            if (!success) return Results.BadRequest(new { error = "No se pudo reenviar la invitación." });
            return Results.Ok(new { message = "Reenvío de invitación puesto en cola correctamente." });
        });

        group.MapPost("/invitations/retry/{outboxId}", async (Guid outboxId, IMeetingAccessService meetingAccessService) =>
        {
            var success = await meetingAccessService.RetryOutboxMessageAsync(outboxId);
            if (!success) return Results.NotFound(new { error = "No se encontró el mensaje de correo en la bandeja de salida." });
            return Results.Ok(new { message = "Reintento de mensaje de correo puesto en cola correctamente." });
        });
    }

    private static string GetBaseUrl(HttpContext httpContext)
    {
        var origin = httpContext.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrWhiteSpace(origin)) return origin;

        var referer = httpContext.Request.Headers["Referer"].ToString();
        if (!string.IsNullOrWhiteSpace(referer))
        {
            var uri = new Uri(referer);
            return $"{uri.Scheme}://{uri.Authority}";
        }

        return "http://localhost:5173";
    }
}
