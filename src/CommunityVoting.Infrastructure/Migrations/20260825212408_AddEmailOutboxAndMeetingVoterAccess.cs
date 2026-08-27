using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityVoting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailOutboxAndMeetingVoterAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Meetings_CommunityId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_CommunityMembers_UserId",
                table: "CommunityMembers");

            migrationBuilder.DropIndex(
                name: "IX_CommunityInvitations_CommunityId",
                table: "CommunityInvitations");

            migrationBuilder.AlterColumn<bool>(
                name: "RequireQuorumForVoting",
                table: "VotingSettings",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "QuorumType",
                table: "VotingSettings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuorumPercentage",
                table: "VotingSettings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "QuorumEnabled",
                table: "VotingSettings",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultMajorityType",
                table: "VotingSettings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "DefaultMajorityPercentage",
                table: "VotingSettings",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AbstentionPolicy",
                table: "VotingSettings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "VotingSettings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Proposals",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Proposals",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "MeetingId",
                table: "Proposals",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "MajorityType",
                table: "Proposals",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MajorityPercentage",
                table: "Proposals",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Proposals",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "AgendaItemId",
                table: "Proposals",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Proposals",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProposalId",
                table: "ProposalOptions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "ProposalOptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ProposalOptions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "VotingStart",
                table: "Meetings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "VotingSettingsId",
                table: "Meetings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "VotingEnd",
                table: "Meetings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Meetings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Meetings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SecondCallAt",
                table: "Meetings",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ScheduledAt",
                table: "Meetings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Meetings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTransparent",
                table: "Meetings",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommunityId",
                table: "Meetings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Meetings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "MeetingParticipants",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "MeetingId",
                table: "MeetingParticipants",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "MeetingParticipants",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPresent",
                table: "MeetingParticipants",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MeetingParticipants",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "UploadedByUserId",
                table: "Documents",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "StoragePath",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProposalId",
                table: "Documents",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<long>(
                name: "FileSize",
                table: "Documents",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Documents",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "CommunityMembers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "MemberRole",
                table: "CommunityMembers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "CommunityMembers",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CommunityMembers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "HasVotingRights",
                table: "CommunityMembers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommunityId",
                table: "CommunityMembers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CommunityMembers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "UsesCount",
                table: "CommunityInvitations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "CommunityInvitations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "MaxUses",
                table: "CommunityInvitations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CommunityInvitations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "CommunityInvitations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "CommunityInvitations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CommunityInvitations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommunityId",
                table: "CommunityInvitations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "CommunityInvitations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "VotingSettingsId",
                table: "Communities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Communities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "Communities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Cif",
                table: "Communities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Communities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Communities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "AgendaItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "AgendaItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "MeetingId",
                table: "AgendaItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AgendaItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "AgendaItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "EmailOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Recipient = table.Column<string>(type: "text", nullable: false),
                    Template = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutboxMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailOutboxMessages_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailOutboxMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MeetingVoterAccesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    CodeHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingVoterAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingVoterAccesses_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingVoterAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CommunityId_ScheduledAt",
                table: "Meetings",
                columns: new[] { "CommunityId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_CommunityId_IsActive",
                table: "CommunityMembers",
                columns: new[] { "CommunityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_UserId_IsActive",
                table: "CommunityMembers",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvitations_CommunityId_IsActive",
                table: "CommunityInvitations",
                columns: new[] { "CommunityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutboxMessages_MeetingId",
                table: "EmailOutboxMessages",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutboxMessages_Status_NextAttemptAt",
                table: "EmailOutboxMessages",
                columns: new[] { "Status", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutboxMessages_UserId",
                table: "EmailOutboxMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingVoterAccesses_MeetingId_UserId",
                table: "MeetingVoterAccesses",
                columns: new[] { "MeetingId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingVoterAccesses_TokenHash",
                table: "MeetingVoterAccesses",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingVoterAccesses_UserId",
                table: "MeetingVoterAccesses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailOutboxMessages");

            migrationBuilder.DropTable(
                name: "MeetingVoterAccesses");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CommunityId_ScheduledAt",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_CommunityMembers_CommunityId_IsActive",
                table: "CommunityMembers");

            migrationBuilder.DropIndex(
                name: "IX_CommunityMembers_UserId_IsActive",
                table: "CommunityMembers");

            migrationBuilder.DropIndex(
                name: "IX_CommunityInvitations_CommunityId_IsActive",
                table: "CommunityInvitations");

            migrationBuilder.AlterColumn<int>(
                name: "RequireQuorumForVoting",
                table: "VotingSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "QuorumType",
                table: "VotingSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "QuorumPercentage",
                table: "VotingSettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "QuorumEnabled",
                table: "VotingSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultMajorityType",
                table: "VotingSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DefaultMajorityPercentage",
                table: "VotingSettings",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AbstentionPolicy",
                table: "VotingSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "VotingSettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Proposals",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Proposals",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "MeetingId",
                table: "Proposals",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "MajorityType",
                table: "Proposals",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MajorityPercentage",
                table: "Proposals",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Proposals",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AgendaItemId",
                table: "Proposals",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Proposals",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "ProposalId",
                table: "ProposalOptions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "ProposalOptions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ProposalOptions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "VotingStart",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "VotingSettingsId",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "VotingEnd",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Meetings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SecondCallAt",
                table: "Meetings",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ScheduledAt",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "IsTransparent",
                table: "Meetings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "CommunityId",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "MeetingParticipants",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "MeetingId",
                table: "MeetingParticipants",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "JoinedAt",
                table: "MeetingParticipants",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "IsPresent",
                table: "MeetingParticipants",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "MeetingParticipants",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UploadedByUserId",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UploadedAt",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "StoragePath",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProposalId",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "FileSize",
                table: "Documents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CommunityMembers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "MemberRole",
                table: "CommunityMembers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "JoinedAt",
                table: "CommunityMembers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "CommunityMembers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "HasVotingRights",
                table: "CommunityMembers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "CommunityId",
                table: "CommunityMembers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "CommunityMembers",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "UsesCount",
                table: "CommunityInvitations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "CommunityInvitations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "MaxUses",
                table: "CommunityInvitations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "CommunityInvitations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "ExpiresAt",
                table: "CommunityInvitations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "CommunityInvitations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedAt",
                table: "CommunityInvitations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "CommunityId",
                table: "CommunityInvitations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "CommunityInvitations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "VotingSettingsId",
                table: "Communities",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Communities",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Communities",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Cif",
                table: "Communities",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Communities",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Communities",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "AgendaItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "AgendaItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "MeetingId",
                table: "AgendaItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AgendaItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AgendaItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CommunityId",
                table: "Meetings",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_UserId",
                table: "CommunityMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvitations_CommunityId",
                table: "CommunityInvitations",
                column: "CommunityId");
        }
    }
}
