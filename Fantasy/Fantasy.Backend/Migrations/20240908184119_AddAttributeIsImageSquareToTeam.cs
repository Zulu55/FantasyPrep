using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fantasy.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeIsImageSquareToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsImageSquare",
                table: "Teams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImageSquare",
                table: "Teams");
        }
    }
}