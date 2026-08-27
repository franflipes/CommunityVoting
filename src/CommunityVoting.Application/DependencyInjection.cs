using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityVoting.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers application services required by Document.Api.
    /// </summary>
    public static IServiceCollection AddDocumentApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        return services;
    }

    /// <summary>
    /// Registers application services required by main API (CommunityVoting.API).
    /// </summary>
    public static IServiceCollection AddMainApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICommunityService, CommunityService>();
        services.AddScoped<IMeetingService, MeetingService>();
        services.AddScoped<IAgendaItemService, AgendaItemService>();
        services.AddScoped<IProposalService, ProposalService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IMeetingAccessService, MeetingAccessService>();
        return services;
    }

    /// <summary>
    /// Registers all application services across the solution.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddDocumentApplicationServices();
        services.AddMainApplicationServices();
        return services;
    }
}
