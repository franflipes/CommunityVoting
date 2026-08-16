using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityVoting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuorumAndMajoritySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MajorityPercentage",
                table: "Proposals",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MajorityType",
                table: "Proposals",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VotingSettingsId",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "HasVotingRights",
                table: "CommunityMembers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CommunityMembers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "VotingSettingsId",
                table: "Communities",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MeetingParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MeetingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsPresent = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingParticipants_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VotingSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuorumEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    QuorumType = table.Column<int>(type: "INTEGER", nullable: false),
                    QuorumPercentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    RequireQuorumForVoting = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultMajorityType = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultMajorityPercentage = table.Column<decimal>(type: "TEXT", nullable: true),
                    AbstentionPolicy = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_VotingSettingsId",
                table: "Meetings",
                column: "VotingSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_VotingSettingsId",
                table: "Communities",
                column: "VotingSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingParticipants_MeetingId_UserId",
                table: "MeetingParticipants",
                columns: new[] { "MeetingId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingParticipants_UserId",
                table: "MeetingParticipants",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_VotingSettings_VotingSettingsId",
                table: "Communities",
                column: "VotingSettingsId",
                principalTable: "VotingSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_VotingSettings_VotingSettingsId",
                table: "Meetings",
                column: "VotingSettingsId",
                principalTable: "VotingSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Communities_VotingSettings_VotingSettingsId",
                table: "Communities");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_VotingSettings_VotingSettingsId",
                table: "Meetings");

            migrationBuilder.DropTable(
                name: "MeetingParticipants");

            migrationBuilder.DropTable(
                name: "VotingSettings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_VotingSettingsId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Communities_VotingSettingsId",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "MajorityPercentage",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "MajorityType",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "VotingSettingsId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "HasVotingRights",
                table: "CommunityMembers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CommunityMembers");

            migrationBuilder.DropColumn(
                name: "VotingSettingsId",
                table: "Communities");
        }
    }
}
