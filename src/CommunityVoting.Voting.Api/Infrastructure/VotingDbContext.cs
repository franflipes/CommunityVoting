using System.Text.Json;
using CommunityVoting.Voting.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Voting.Api.Infrastructure;

public class VotingDbContext : DbContext
{
    public VotingDbContext(DbContextOptions<VotingDbContext> options) : base(options) { }

    public DbSet<ProposalVotingSession> Sessions => Set<ProposalVotingSession>();
    public DbSet<VotingResult> Results => Set<VotingResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("voting");

        modelBuilder.Entity<ProposalVotingSession>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Title).IsRequired();
            b.OwnsMany(s => s.Options, opts => opts.ToJson());
            b.OwnsMany(s => s.Ballots, ballots =>
            {
                ballots.ToJson();
                ballots.OwnsOne(b => b.Vote);
            });
            b.HasIndex(s => s.ProposalId);
            b.HasIndex(s => s.MeetingId);
        });

        modelBuilder.Entity<VotingResult>(b =>
        {
            b.HasKey(r => r.Id);
            b.HasIndex(r => r.OriginalProposalId);
            b.HasIndex(r => r.MeetingId);
            b.Property(r => r.VotesByOption)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, double>>(v, (JsonSerializerOptions?)null) ?? new()
                );
        });
    }
}
