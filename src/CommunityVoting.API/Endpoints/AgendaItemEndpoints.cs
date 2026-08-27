using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;

namespace CommunityVoting.API.Endpoints;

public static class AgendaItemEndpoints
{
    public static void MapAgendaItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/agenda-items").RequireAuthorization();

        group.MapPost("/", async (CreateAgendaItemRequest request, IAgendaItemService agendaItemService) =>
        {
            var item = await agendaItemService.CreateAsync(request);
            if (item == null) return Results.BadRequest("Reunión no encontrada.");
            return Results.Created($"/api/agenda-items/{item.Id}", item);
        });

        group.MapGet("/meeting/{meetingId}", async (Guid meetingId, IAgendaItemService agendaItemService) =>
        {
            var items = await agendaItemService.GetByMeetingIdAsync(meetingId);
            return Results.Ok(items);
        });

        group.MapGet("/{id}", async (Guid id, IAgendaItemService agendaItemService) =>
        {
            var item = await agendaItemService.GetByIdAsync(id);
            if (item == null) return Results.NotFound();
            return Results.Ok(item);
        });

        group.MapDelete("/{id}", async (Guid id, IAgendaItemService agendaItemService) =>
        {
            var success = await agendaItemService.DeleteAsync(id);
            if (!success) return Results.NotFound();
            return Results.NoContent();
        });
    }
}
