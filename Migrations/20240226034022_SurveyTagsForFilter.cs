using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SHL_Platform.Migrations
{
    /// <inheritdoc />
    public partial class SurveyTagsForFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SurveyTags",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SurveyTags",
                table: "Surveys");
        }
    }
}
