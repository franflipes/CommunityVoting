using System.Text;
using CommunityVoting.Document.Api.Endpoints;
using CommunityVoting.Infrastructure;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using OpenTelemetry;
using OpenTelemetry.Trace;
using CommunityVoting.Application;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register Database & Document Infrastructure (DbContext, IDocumentRepository, IProposalRepository, IUserRepository, IFileStorageService)
builder.Services.AddDatabaseInfrastructure(builder.Configuration);
builder.Services.AddDocumentInfrastructure(builder.Configuration);

// Register Document Application Services (DocumentService)
builder.Services.AddDocumentApplicationServices();

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
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapDocumentEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommunityVotingDbContext>();
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    await db.WaitForDatabaseAsync(logger);
}

app.Run();
