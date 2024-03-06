using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SHL_Platform.Migrations
{
    /// <inheritdoc />
    public partial class BccAndCCAdding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bcc",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cc",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bcc",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "Cc",
                table: "Surveys");
        }
    }
}
