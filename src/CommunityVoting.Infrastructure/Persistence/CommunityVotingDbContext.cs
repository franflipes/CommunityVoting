using CommunityVoting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Persistence;

public class CommunityVotingDbContext : DbContext
{
    public CommunityVotingDbContext(DbContextOptions<CommunityVotingDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CommunityMember> CommunityMembers => Set<CommunityMember>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<AgendaItem> AgendaItems => Set<AgendaItem>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalOption> ProposalOptions => Set<ProposalOption>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<VotingSettings> VotingSettings => Set<VotingSettings>();
    public DbSet<MeetingParticipant> MeetingParticipants => Set<MeetingParticipant>();
    public DbSet<CommunityInvitation> CommunityInvitations => Set<CommunityInvitation>();
    public DbSet<MeetingVoterAccess> MeetingVoterAccesses => Set<MeetingVoterAccess>();
    public DbSet<EmailOutboxMessage> EmailOutboxMessages => Set<EmailOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Community -> CreatedBy User
        modelBuilder.Entity<Community>()
            .HasOne(c => c.CreatedBy)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Community -> VotingSettings
        modelBuilder.Entity<Community>()
            .HasOne(c => c.VotingSettings)
            .WithMany()
            .HasForeignKey(c => c.VotingSettingsId)
            .OnDelete(DeleteBehavior.Restrict);

        // CommunityMember configuration
        modelBuilder.Entity<CommunityMember>()
            .HasOne(cm => cm.Community)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommunityMember>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommunityMember>()
            .HasIndex(cm => new { cm.CommunityId, cm.UserId })
            .IsUnique();

        // Meeting -> Community
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.Community)
            .WithMany(c => c.Meetings)
            .HasForeignKey(m => m.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Meeting -> VotingSettings
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.VotingSettings)
            .WithMany()
            .HasForeignKey(m => m.VotingSettingsId)
            .OnDelete(DeleteBehavior.Restrict);

        // MeetingParticipant configuration
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.Meeting)
            .WithMany(m => m.Participants)
            .HasForeignKey(mp => mp.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.User)
            .WithMany()
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MeetingParticipant>()
            .HasIndex(mp => new { mp.MeetingId, mp.UserId })
            .IsUnique();

        // AgendaItem -> Meeting (1:N)
        modelBuilder.Entity<AgendaItem>()
            .HasOne(ai => ai.Meeting)
            .WithMany(m => m.AgendaItems)
            .HasForeignKey(ai => ai.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Proposal -> AgendaItem (1:N)
        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.AgendaItem)
            .WithMany(ai => ai.Proposals)
            .HasForeignKey(p => p.AgendaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Proposal -> Meeting (1:N)
        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.Meeting)
            .WithMany(m => m.Proposals)
            .HasForeignKey(p => p.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProposalOption -> Proposal (1:N)
        modelBuilder.Entity<ProposalOption>()
            .HasOne(po => po.Proposal)
            .WithMany(p => p.Options)
            .HasForeignKey(po => po.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Document -> Proposal (1:N)
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Proposal)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Document -> UploadedByUser
        modelBuilder.Entity<Document>()
            .HasOne(d => d.UploadedByUser)
            .WithMany()
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // CommunityInvitation configuration
        modelBuilder.Entity<CommunityInvitation>()
            .HasOne(ci => ci.Community)
            .WithMany(c => c.Invitations)
            .HasForeignKey(ci => ci.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommunityInvitation>()
            .HasOne(ci => ci.CreatedByUser)
            .WithMany()
            .HasForeignKey(ci => ci.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CommunityInvitation>()
            .HasIndex(ci => ci.Token)
            .IsUnique();

        // Database Performance Indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Meeting>()
            .HasIndex(m => new { m.CommunityId, m.ScheduledAt });

        modelBuilder.Entity<CommunityMember>()
            .HasIndex(cm => new { cm.UserId, cm.IsActive });

        modelBuilder.Entity<CommunityMember>()
            .HasIndex(cm => new { cm.CommunityId, cm.IsActive });

        modelBuilder.Entity<Proposal>()
            .HasIndex(p => p.MeetingId);

        modelBuilder.Entity<Proposal>()
            .HasIndex(p => p.AgendaItemId);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.ProposalId);

        modelBuilder.Entity<CommunityInvitation>()
            .HasIndex(ci => new { ci.CommunityId, ci.IsActive });

        // MeetingVoterAccess configuration
        modelBuilder.Entity<MeetingVoterAccess>()
            .HasOne(mva => mva.Meeting)
            .WithMany(m => m.VoterAccesses)
            .HasForeignKey(mva => mva.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MeetingVoterAccess>()
            .HasOne(mva => mva.User)
            .WithMany()
            .HasForeignKey(mva => mva.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MeetingVoterAccess>()
            .HasIndex(mva => new { mva.MeetingId, mva.UserId })
            .IsUnique();

        modelBuilder.Entity<MeetingVoterAccess>()
            .HasIndex(mva => mva.TokenHash);

        // EmailOutboxMessage configuration
        modelBuilder.Entity<EmailOutboxMessage>()
            .HasOne(e => e.Meeting)
            .WithMany()
            .HasForeignKey(e => e.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmailOutboxMessage>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EmailOutboxMessage>()
            .HasIndex(e => new { e.Status, e.NextAttemptAt });

        modelBuilder.Entity<EmailOutboxMessage>()
            .HasIndex(e => e.MeetingId);
    }
}
