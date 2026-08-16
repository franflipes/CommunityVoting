using System.Security.Claims;
using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Services;

namespace CommunityVoting.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest request, AuthService authService) =>
        {
            var response = await authService.RegisterAsync(request);
            if (response == null) return Results.BadRequest(new { error = "El usuario con este correo electrónico ya existe." });
            return Results.Ok(response);
        });

        group.MapPost("/login", async (LoginRequest request, AuthService authService) =>
        {
            var response = await authService.LoginAsync(request);
            if (response == null) return Results.Unauthorized();
            return Results.Ok(response);
        });

        group.MapGet("/me", async (HttpContext httpContext, AuthService authService) =>
        {
            var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var user = await authService.GetProfileAsync(userId);
            if (user == null) return Results.NotFound();
            return Results.Ok(user);
        }).RequireAuthorization();
    }
}
