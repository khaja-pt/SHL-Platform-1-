using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SHL_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddingMailAndUniqueID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UniqueId",
                table: "Surveys",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "UniqueId",
                table: "Surveys");
        }
    }
}
