using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameCatalogue.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameCoverImageToKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Games");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageKey",
                table: "Games",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageKey",
                table: "Games");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Games",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }
    }
}
