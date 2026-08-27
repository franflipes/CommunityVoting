using System.Text;
using CommunityVoting.Infrastructure.Persistence;
using CommunityVoting.Voting.Api.Endpoints;
using CommunityVoting.Voting.Api.Hubs;
using CommunityVoting.Voting.Api.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add PostgreSQL DbContext for persistent voting sessions in 'voting' schema
var postgresConnStr = builder.Configuration.GetConnectionString("postgres")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5433;Database=communityvoting;Username=postgres;Password=postgres";

builder.Services.AddDbContext<VotingDbContext>(options =>
    options.UseNpgsql(postgresConnStr, b =>
        b.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)));

builder.Services.AddScoped<IVotingRepository, EfVotingRepository>();
builder.Services.AddScoped<VotingRuntimeManager>();
builder.Services.AddSignalR();

// Add HttpClient for calling CommunityVoting.API
builder.Services.AddHttpClient("CommunityVotingApi", client =>
{
    var apiBaseUrl = builder.Configuration["services:main-api:http:0"] 
        ?? builder.Configuration["ApiBaseUrl"] 
        ?? "http://localhost:5004";
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JWT Authentication
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "SuperSecretKeyForCommunityVotingApp12345!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "CommunityVoting";
var audience = builder.Configuration["Jwt:Audience"] ?? "CommunityVotingApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        // Support JWT Token in SignalR Query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/voting"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VotingDbContext>();
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    await db.WaitForDatabaseAsync(logger);
    await db.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS voting;");
    var creator = db.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
    try
    {
        await creator.CreateTablesAsync();
    }
    catch
    {
        // Tables already created
    }
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<VotingHub>("/hubs/voting");
app.MapVotingEndpoints();

app.Run();
