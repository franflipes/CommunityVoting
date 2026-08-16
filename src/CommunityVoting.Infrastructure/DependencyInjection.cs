using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Infrastructure.Authentication;
using CommunityVoting.Infrastructure.Persistence;
using CommunityVoting.Infrastructure.Repositories;
using CommunityVoting.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityVoting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("postgres")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=postgres;Username=postgres;Password=postgres";

        services.AddDbContext<CommunityVotingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(CommunityVotingDbContext).Assembly.FullName));
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICommunityRepository, CommunityRepository>();
        services.AddScoped<IMeetingRepository, MeetingRepository>();
        services.AddScoped<IAgendaItemRepository, AgendaItemRepository>();
        services.AddScoped<IProposalRepository, ProposalRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ICommunityInvitationRepository, CommunityInvitationRepository>();

        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        services.AddScoped<AuthService>();
        services.AddScoped<CommunityService>();
        services.AddScoped<MeetingService>();
        services.AddScoped<AgendaItemService>();
        services.AddScoped<ProposalService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<InvitationService>();

        return services;
    }
}
