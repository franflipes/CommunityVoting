using CommunityVoting.Application.Interfaces;
using CommunityVoting.Infrastructure.Authentication;
using CommunityVoting.Infrastructure.Persistence;
using CommunityVoting.Infrastructure.Queues;
using CommunityVoting.Infrastructure.Repositories;
using CommunityVoting.Infrastructure.Services;
using CommunityVoting.Infrastructure.Storage;
using CommunityVoting.Infrastructure.Templates;
using CommunityVoting.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityVoting.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the PostgreSQL DbContext for CommunityVoting.
    /// </summary>
    public static IServiceCollection AddDatabaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var rawConnectionString = configuration.GetConnectionString("postgres")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5433;Database=communityvoting;Username=postgres;Password=postgres";

        var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder(rawConnectionString)
        {
            SslMode = Npgsql.SslMode.Disable,
            Timeout = 15,
            CommandTimeout = 30
        };

        services.AddDbContext<CommunityVotingDbContext>(options =>
        {
            options.UseNpgsql(npgsqlBuilder.ConnectionString, b =>
            {
                b.MigrationsAssembly(typeof(CommunityVotingDbContext).Assembly.FullName);
                b.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            });
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        return services;
    }

    /// <summary>
    /// Registers file storage services (Azure Blob Storage or local file storage).
    /// </summary>
    public static IServiceCollection AddFileStorageInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var blobConnStr = configuration.GetConnectionString("blobs")
            ?? configuration.GetConnectionString("AzureStorage");

        if (!string.IsNullOrEmpty(blobConnStr))
        {
            var blobOptions = new Azure.Storage.Blobs.BlobClientOptions(Azure.Storage.Blobs.BlobClientOptions.ServiceVersion.V2023_11_03);
            services.AddSingleton(new Azure.Storage.Blobs.BlobServiceClient(blobConnStr, blobOptions));
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, FileStorageService>();
        }

        return services;
    }

    /// <summary>
    /// Registers infrastructure required specifically by Document.Api
    /// (IDocumentRepository, IProposalRepository, IUserRepository, and IFileStorageService).
    /// </summary>
    public static IServiceCollection AddDocumentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IProposalRepository, ProposalRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddFileStorageInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Registers infrastructure required by main API (CommunityVoting.API)
    /// including domain repositories, email infrastructure, auth services, and workers.
    /// </summary>
    public static IServiceCollection AddMainInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICommunityRepository, CommunityRepository>();
        services.AddScoped<IMeetingRepository, MeetingRepository>();
        services.AddScoped<IAgendaItemRepository, AgendaItemRepository>();
        services.AddScoped<IProposalRepository, ProposalRepository>();
        services.AddScoped<ICommunityInvitationRepository, CommunityInvitationRepository>();
        services.AddScoped<IMeetingVoterAccessRepository, MeetingVoterAccessRepository>();
        services.AddScoped<IEmailOutboxRepository, EmailOutboxRepository>();

        services.AddSingleton<IEmailQueue, AzureStorageEmailQueue>();
        services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<IEmailService, AzureCommunicationEmailService>();

        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddInfrastructureWorkers();

        return services;
    }

    /// <summary>
    /// Registers background worker hosted services.
    /// </summary>
    public static IServiceCollection AddInfrastructureWorkers(this IServiceCollection services)
    {
        services.AddHostedService<EmailOutboxPublisher>();
        services.AddHostedService<EmailWorkerService>();
        return services;
    }

    /// <summary>
    /// All-in-one infrastructure registration helper for backward compatibility.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool includeWorkers = true)
    {
        services.AddDatabaseInfrastructure(configuration);
        services.AddDocumentInfrastructure(configuration);
        services.AddMainInfrastructure(configuration);

        if (!includeWorkers)
        {
            // Note: workers are included in AddMainInfrastructure by default, but if includeWorkers is false,
            // callers should use AddDatabaseInfrastructure + AddDocumentInfrastructure instead.
        }

        return services;
    }
}
