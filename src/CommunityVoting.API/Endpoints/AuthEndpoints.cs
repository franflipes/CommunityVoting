using System.Security.Claims;
using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;

namespace CommunityVoting.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            var response = await authService.RegisterAsync(request);
            if (response == null) return Results.BadRequest(new { error = "El usuario con este correo electrónico ya existe." });
            return Results.Ok(response);
        });

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var response = await authService.LoginAsync(request);
            if (response == null) return Results.Unauthorized();
            return Results.Ok(response);
        });

        group.MapGet("/me", async (HttpContext httpContext, IAuthService authService) =>
        {
            var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var user = await authService.GetProfileAsync(userId);
            if (user == null) return Results.NotFound();
            return Results.Ok(user);
        }).RequireAuthorization();

        group.MapPost("/reset-password", async (ResetPasswordRequest request, IAuthService authService) =>
        {
            var success = await authService.ResetPasswordAsync(request);
            if (!success) return Results.BadRequest(new { error = "No se encontró ningún usuario registrado con ese correo electrónico." });
            return Results.Ok(new { message = "Contraseña restablecida correctamente." });
        });

        group.MapPost("/change-password", async (ChangePasswordRequest request, HttpContext httpContext, IAuthService authService) =>
        {
            var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var success = await authService.ChangePasswordAsync(userId, request);
            if (!success) return Results.BadRequest(new { error = "La contraseña actual introducida no es correcta." });
            return Results.Ok(new { message = "Contraseña actualizada correctamente." });
        }).RequireAuthorization();
    }
}
