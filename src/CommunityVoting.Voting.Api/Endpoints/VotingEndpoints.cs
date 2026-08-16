using System.Security.Claims;
using CommunityVoting.Voting.Api.Domain;
using CommunityVoting.Voting.Api.Hubs;
using CommunityVoting.Voting.Api.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace CommunityVoting.Voting.Api.Endpoints;

public static class VotingEndpoints
{
    public static void MapVotingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/voting");

        // 1. Create session (Internal or Admin trigger)
        group.MapPost("/sessions", async (CreateSessionRequest request, IVotingRepository repository, IHubContext<VotingHub> hubContext) =>
        {
            var existing = await repository.GetSessionByProposalIdAsync(request.ProposalId);
            if (existing != null)
            {
                return Results.BadRequest(new { error = "Ya existe una sesión de votación para esta propuesta.", session = MapToLiveSessionDto(existing) });
            }

            var session = new ProposalVotingSession
            {
                Id = Guid.NewGuid(),
                ProposalId = request.ProposalId,
                MeetingId = request.MeetingId,
                CommunityId = request.CommunityId,
                AgendaItemId = request.AgendaItemId,
                AgendaItemTitle = request.AgendaItemTitle ?? string.Empty,
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                DisplayOrder = request.DisplayOrder,
                MeetingName = request.MeetingName,
                State = VotingState.Created
            };

            session.MajorityType = request.MajorityType ?? 1;
            session.MajorityPercentage = request.MajorityPercentage;

            foreach (var opt in request.Options)
            {
                session.Options.Add(new VoteOption { Id = opt.Id, Label = opt.Label });
            }

            if (!session.Options.Any(o => o.Label.Equals("Abstención", StringComparison.OrdinalIgnoreCase) || o.Label.Equals("Abstenerse", StringComparison.OrdinalIgnoreCase)))
            {
                session.Options.Add(new VoteOption { Id = Guid.NewGuid(), Label = "Abstención" });
            }

            await repository.AddSessionAsync(session);

            await hubContext.Clients.Groups(new[] { $"meeting-{session.MeetingId}", $"session-{session.Id}" }).SendAsync("OnSessionCreated", new
            {
                ProposalId = session.ProposalId,
                SessionId = session.Id,
                State = session.State
            });

            return Results.Created($"/api/voting/sessions/{session.Id}", MapToLiveSessionDto(session));
        });

        // 2. Prepare session (Admin)
        group.MapPost("/sessions/{id}/prepare", async (Guid id, IVotingRepository repository, IHttpClientFactory httpClientFactory, IHubContext<VotingHub> hubContext, HttpContext httpContext) =>
        {
            var session = await repository.GetSessionAsync(id) ?? await repository.GetSessionByProposalIdAsync(id);
            if (session == null) return Results.NotFound("Sesión de votación no encontrada.");

            var client = httpClientFactory.CreateClient("CommunityVotingApi");
            if (httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader.ToString());
            }

            try
            {
                var response = await client.GetAsync($"/api/meetings/{session.MeetingId}/voting-eligible-data");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.BadRequest($"No se pudieron cargar los datos de votantes elegibles: {response.ReasonPhrase}");
                }

                var data = await response.Content.ReadFromJsonAsync<EligibleDataResponse>();
                if (data == null) return Results.BadRequest("Respuesta vacía del servidor API.");

                session.EligibleMembers = data.EligibleMembers;
                session.PresentMembers = data.PresentMembers;
                session.QuorumRequired = data.QuorumRequired;
                session.QuorumReached = data.QuorumReached;

                session.Ballots.Clear();
                foreach (var voter in data.Voters)
                {
                    var ballot = new Ballot
                    {
                        Id = Guid.NewGuid(),
                        UserId = voter.UserId,
                        Name = voter.Name,
                        LastName = voter.LastName,
                        Email = voter.Email,
                        Status = BallotStatus.Pending
                    };
                    session.Ballots.Add(ballot);
                }

                session.State = VotingState.Prepared;
                await repository.UpdateSessionAsync(session);

                await hubContext.Clients.Groups(new[] { $"meeting-{session.MeetingId}", $"session-{session.Id}" }).SendAsync("OnSessionPrepared", new
                {
                    ProposalId = session.ProposalId,
                    SessionId = session.Id,
                    State = session.State
                });

                return Results.Ok(MapToLiveSessionDto(session));
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Error al conectar con el servicio de API: {ex.Message}");
            }
        }).RequireAuthorization();

        // 3. Open session (Admin)
        group.MapPost("/sessions/{id}/open", async (Guid id, IVotingRepository repository, IHubContext<VotingHub> hubContext) =>
        {
            var session = await repository.GetSessionAsync(id) ?? await repository.GetSessionByProposalIdAsync(id);
            if (session == null) return Results.NotFound("Sesión de votación no encontrada.");

            if (session.RequireQuorumForVoting && !session.QuorumReached)
            {
                return Results.BadRequest($"No se puede iniciar la votación porque no se ha alcanzado el quórum mínimo requerido. Asistentes presentes: {session.PresentMembers}/{session.EligibleMembers} (Mínimo requerido: {session.QuorumRequired}).");
            }

            session.State = VotingState.Open;
            session.OpenedAt = DateTime.UtcNow;
            session.ExpirationTime = DateTime.UtcNow.AddHours(2);

            await repository.UpdateSessionAsync(session);

            await hubContext.Clients.Groups(new[] { $"meeting-{session.MeetingId}", $"session-{session.Id}" }).SendAsync("OnSessionOpened", new
            {
                ProposalId = session.ProposalId,
                SessionId = session.Id,
                OpenedAt = session.OpenedAt,
                ExpirationTime = session.ExpirationTime
            });

            return Results.Ok(MapToLiveSessionDto(session));
        }).RequireAuthorization();

        // 4. Close session (Admin)
        group.MapPost("/sessions/{id}/close", async (Guid id, IVotingRepository repository, VotingRuntimeManager runtimeManager, HttpContext httpContext) =>
        {
            var session = await repository.GetSessionAsync(id) ?? await repository.GetSessionByProposalIdAsync(id);
            if (session == null) return Results.NotFound("Sesión de votación no encontrada.");

            var userName = httpContext.User.Identity?.Name ?? "Administrador";
            await runtimeManager.CloseSessionAsync(session, userName);
            return Results.Ok(new { message = "Sesión de votación cerrada correctamente." });
        }).RequireAuthorization();

        // 5. Vote (User)
        group.MapPost("/sessions/{id}/vote", async (Guid id, CastVoteRequest request, IVotingRepository repository, IHubContext<VotingHub> hubContext, HttpContext httpContext) =>
        {
            var session = await repository.GetSessionAsync(id) ?? await repository.GetSessionByProposalIdAsync(id);
            if (session == null) return Results.NotFound("Sesión de votación no encontrada.");
            if (session.State != VotingState.Open) return Results.BadRequest("La votación no está abierta.");

            var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var callerUserId))
            {
                return Results.Unauthorized();
            }

            var ballot = session.Ballots.FirstOrDefault(b => b.UserId == callerUserId || b.Id == request.BallotId);
            if (ballot == null)
            {
                var userName = httpContext.User.FindFirst("name")?.Value ?? httpContext.User.Identity?.Name ?? "Votante";
                ballot = new Ballot
                {
                    Id = Guid.NewGuid(),
                    UserId = callerUserId,
                    Name = userName,
                    Email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    Status = BallotStatus.Pending
                };
                session.Ballots.Add(ballot);
            }

            var option = session.Options.FirstOrDefault(o => o.Id == request.OptionId);
            if (option == null) return Results.BadRequest("La opción de voto seleccionada no es válida.");

            ballot.Vote = new Vote
            {
                SelectedOptionId = option.Id,
                SelectedOptionLabel = option.Label,
                CastAt = DateTime.UtcNow
            };
            ballot.Status = BallotStatus.Voted;

            await repository.UpdateSessionAsync(session);

            int totalBallots = session.Ballots.Count;
            int totalVotesCast = session.Ballots.Count(b => b.Status == BallotStatus.Voted);
            double participationPercentage = totalBallots > 0 ? ((double)totalVotesCast / totalBallots) * 100.0 : 0.0;

            var votesByOption = session.Options.ToDictionary(o => o.Id.ToString(), _ => 0.0);
            foreach (var b in session.Ballots.Where(v => v.Status == BallotStatus.Voted && v.Vote != null))
            {
                var optId = b.Vote!.SelectedOptionId.ToString();
                if (!votesByOption.ContainsKey(optId)) votesByOption[optId] = 0.0;
                votesByOption[optId] += 1.0;
            }

            await hubContext.Clients.Groups(new[] { $"meeting-{session.MeetingId}", $"session-{session.Id}" }).SendAsync("OnVoteCast", new
            {
                ProposalId = session.ProposalId,
                SessionId = session.Id,
                BallotId = ballot.Id,
                UserId = ballot.UserId,
                Status = ballot.Status,
                LiveStats = new
                {
                    TotalBallots = totalBallots,
                    TotalVotesCast = totalVotesCast,
                    ParticipationPercentage = participationPercentage,
                    VotesByOption = votesByOption
                }
            });

            return Results.Ok(new { message = "Voto registrado correctamente", ballotStatus = ballot.Status });
        }).RequireAuthorization();

        // 6. Get session details & live stats
        group.MapGet("/sessions/{id}", async (Guid id, IVotingRepository repository) =>
        {
            var session = await repository.GetSessionAsync(id) ?? await repository.GetSessionByProposalIdAsync(id);
            if (session == null) return Results.NotFound("Sesión de votación no encontrada.");

            return Results.Ok(MapToLiveSessionDto(session));
        });

        // 7. Get historical result
        group.MapGet("/results/{id}", async (Guid id, IVotingRepository repository) =>
        {
            var result = await repository.GetResultAsync(id) ?? await repository.GetResultByProposalIdAsync(id);
            if (result == null) return Results.NotFound("Resultado de votación no encontrado.");
            return Results.Ok(result);
        });

        // 8. Get proposal voting statuses for a meeting
        group.MapGet("/meetings/{meetingId}/statuses", async (Guid meetingId, IVotingRepository repository) =>
        {
            var activeSessions = await repository.GetActiveSessionsAsync();
            var meetingActive = activeSessions.Where(s => s.MeetingId == meetingId);
            var closedResults = await repository.GetResultsByMeetingIdAsync(meetingId);

            var statuses = new Dictionary<string, object>();

            foreach (var session in meetingActive)
            {
                statuses[session.ProposalId.ToString()] = new
                {
                    State = (int)session.State,
                    SessionId = session.Id
                };
            }

            foreach (var result in closedResults)
            {
                statuses[result.OriginalProposalId.ToString()] = new
                {
                    State = 3, // Closed
                    ResultId = result.Id
                };
            }

            return Results.Ok(statuses);
        });
    }

    private static object MapToLiveSessionDto(ProposalVotingSession session)
    {
        int totalBallots = session.Ballots.Count;
        int totalVotesCast = session.Ballots.Count(b => b.Status == BallotStatus.Voted);
        double participationPercentage = totalBallots > 0 ? ((double)totalVotesCast / totalBallots) * 100.0 : 0.0;

        var votesByOption = session.Options.ToDictionary(o => o.Id.ToString(), _ => 0.0);
        foreach (var b in session.Ballots.Where(v => v.Status == BallotStatus.Voted && v.Vote != null))
        {
            var optId = b.Vote!.SelectedOptionId.ToString();
            if (!votesByOption.ContainsKey(optId)) votesByOption[optId] = 0.0;
            votesByOption[optId] += 1.0;
        }

        return new
        {
            session.Id,
            session.ProposalId,
            session.MeetingId,
            session.CommunityId,
            session.AgendaItemId,
            session.AgendaItemTitle,
            session.Title,
            session.Description,
            session.DisplayOrder,
            session.MeetingName,
            session.State,
            session.CreatedAt,
            session.OpenedAt,
            session.ClosedAt,
            session.ExpirationTime,
            session.EligibleMembers,
            session.PresentMembers,
            session.QuorumRequired,
            session.QuorumReached,
            session.RequireQuorumForVoting,
            session.MajorityType,
            session.MajorityPercentage,
            Options = session.Options.Select(o => new { o.Id, o.Label }).ToList(),
            Ballots = session.Ballots.Select(b => new
            {
                b.Id,
                b.UserId,
                b.Name,
                b.LastName,
                b.Email,
                Vote = b.Vote == null ? null : new
                {
                    b.Vote.SelectedOptionId,
                    b.Vote.SelectedOptionLabel,
                    b.Vote.CastAt
                },
                b.Status
            }).ToList(),
            LiveStats = new
            {
                TotalBallots = totalBallots,
                TotalVotesCast = totalVotesCast,
                ParticipationPercentage = participationPercentage,
                VotesByOption = votesByOption
            }
        };
    }
}

public record CreateSessionRequest(
    Guid ProposalId,
    Guid MeetingId,
    Guid CommunityId,
    Guid? AgendaItemId,
    string? AgendaItemTitle,
    string Title,
    string? Description,
    int DisplayOrder,
    string MeetingName,
    int? MajorityType,
    double? MajorityPercentage,
    List<VoteOptionRequest> Options
);

public record VoteOptionRequest(Guid Id, string Label);

public record CastVoteRequest(
    Guid? BallotId,
    Guid OptionId
);

public class EligibleDataResponse
{
    public Guid CommunityId { get; set; }
    public Guid MeetingId { get; set; }
    public string MeetingTitle { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int EligibleMembers { get; set; }
    public int PresentMembers { get; set; }
    public int QuorumRequired { get; set; }
    public bool QuorumReached { get; set; }
    public List<EligibleVoterDto> Voters { get; set; } = new();
}

public class EligibleVoterDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
