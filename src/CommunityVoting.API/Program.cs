using System.Text;
using CommunityVoting.API.Endpoints;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using CommunityVoting.Infrastructure;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure & Application Services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
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

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Map Endpoints
app.MapAuthEndpoints();
app.MapCommunityEndpoints();
app.MapMeetingEndpoints();
app.MapAgendaItemEndpoints();
app.MapProposalEndpoints();
app.MapDocumentEndpoints();
app.MapInvitationEndpoints();

// Seed initial demo data in SQLite DB
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CommunityVotingDbContext>();
    context.Database.EnsureCreated();

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    if (!context.Users.Any())
    {
        var admin = User.Create("Carlos", "Administrador", "admin@comunidad.com", "+34600111222", hasher.HashPassword("Admin123!"), UserRole.GlobalAdmin);
        var adminComm = User.Create("Laura", "Gestora", "laura@comunidad.com", "+34611222333", hasher.HashPassword("Admin123!"), UserRole.CommunityAdmin);
        var voter1 = User.Create("Juan", "Pérez", "juan@comunidad.com", "+34622333444", hasher.HashPassword("Voter123!"), UserRole.CommunityMember);
        var voter2 = User.Create("María", "López", "maria@comunidad.com", "+34633444555", hasher.HashPassword("Voter123!"), UserRole.CommunityMember);

        context.Users.AddRange(admin, adminComm, voter1, voter2);
        context.SaveChanges();

        var community = Community.Create("Residencial Las Flores", "Av. Principal 123", "B12345678", adminComm.Id);
        context.Communities.Add(community);
        context.SaveChanges();

        var m1 = CommunityMember.Create(community.Id, adminComm.Id, UserRole.CommunityAdmin);
        var m2 = CommunityMember.Create(community.Id, voter1.Id, UserRole.CommunityMember);
        var m3 = CommunityMember.Create(community.Id, voter2.Id, UserRole.CommunityMember);
        context.CommunityMembers.AddRange(m1, m2, m3);
        context.SaveChanges();

        var meeting = Meeting.Create(community.Id, "Junta General Ordinaria 2026", MeetingType.Ordinary, "Sala de Comunidad / Online", DateTime.UtcNow.AddDays(7));
        context.Meetings.Add(meeting);
        context.SaveChanges();

        // Seed Agenda Items
        var agendaItem1 = AgendaItem.Create(meeting.Id, "Punto 1: Cuentas y Balances Anuales", "Revisión de los estados financieros del ejercicio 2025", 1);
        var agendaItem2 = AgendaItem.Create(meeting.Id, "Punto 2: Obras y Mejoas de Eficiencia Energética", "Proyectos de renovación e instalación de energía solar", 2);
        context.AgendaItems.AddRange(agendaItem1, agendaItem2);
        context.SaveChanges();

        var proposal1 = Proposal.Create(meeting.Id, agendaItem1.Id, "Aprobación de Cuentas Anuales 2025", "Se somete a votación la aprobación del balance financiero y presupuesto del ejercicio anterior.", 1);
        proposal1.Options.Add(ProposalOption.Create(proposal1.Id, "A favor"));
        proposal1.Options.Add(ProposalOption.Create(proposal1.Id, "En contra"));
        proposal1.Options.Add(ProposalOption.Create(proposal1.Id, "Abstención"));

        var proposal2 = Proposal.Create(meeting.Id, agendaItem2.Id, "Instalación de Paneles Solares en Cubierta", "Propuesta para contratar la instalación fotovoltaica comunitaria con subvención europea.", 2);
        proposal2.Options.Add(ProposalOption.Create(proposal2.Id, "Aprobar Proyecto A (10 kW)"));
        proposal2.Options.Add(ProposalOption.Create(proposal2.Id, "Aprobar Proyecto B (20 kW)"));
        proposal2.Options.Add(ProposalOption.Create(proposal2.Id, "Rechazar Instalación"));

        context.Proposals.AddRange(proposal1, proposal2);
        context.SaveChanges();
    }
}

app.Run();
