using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SHL_Platform.Migrations
{
    /// <inheritdoc />
    public partial class SurveyTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Surveys",
                newName: "SendToEmail");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedSurvey",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedSurvey",
                table: "Surveys");

            migrationBuilder.RenameColumn(
                name: "SendToEmail",
                table: "Surveys",
                newName: "Email");
        }
    }
}
