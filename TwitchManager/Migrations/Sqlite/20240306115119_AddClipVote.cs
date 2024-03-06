using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchManager.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddClipVote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClipVotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ClipId = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClipVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClipVotes_Clips_ClipId",
                        column: x => x.ClipId,
                        principalTable: "Clips",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClipVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClipVotes_ClipId",
                table: "ClipVotes",
                column: "ClipId");

            migrationBuilder.CreateIndex(
                name: "IX_ClipVotes_UserId",
                table: "ClipVotes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClipVotes");
        }
    }
}
