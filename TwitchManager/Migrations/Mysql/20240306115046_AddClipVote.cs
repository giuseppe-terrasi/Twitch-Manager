using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchManager.Migrations.Mysql
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
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClipId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
