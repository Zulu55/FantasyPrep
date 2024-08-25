using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fantasy.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TournamentId",
                table: "Groups",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Tournaments_TournamentId",
                table: "Groups",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Tournaments_TournamentId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_TournamentId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Groups");
        }
    }
}
