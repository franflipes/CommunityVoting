using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityVoting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendaItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgendaItemId",
                table: "Proposals",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AgendaItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MeetingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendaItems_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_AgendaItemId",
                table: "Proposals",
                column: "AgendaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItems_MeetingId",
                table: "AgendaItems",
                column: "MeetingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_AgendaItems_AgendaItemId",
                table: "Proposals",
                column: "AgendaItemId",
                principalTable: "AgendaItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_AgendaItems_AgendaItemId",
                table: "Proposals");

            migrationBuilder.DropTable(
                name: "AgendaItems");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_AgendaItemId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "AgendaItemId",
                table: "Proposals");
        }
    }
}
