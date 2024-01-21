using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace timer_app.Migrations
{
    public partial class UpdateProjectColours : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayColour",
                table: "Projects",
                newName: "ProjectColor_Lightest");

            migrationBuilder.AddColumn<string>(
                name: "ProjectColor_Dark",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectColor_Darkest",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectColor_Light",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectColor_Dark",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectColor_Darkest",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectColor_Light",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "ProjectColor_Lightest",
                table: "Projects",
                newName: "DisplayColour");
        }
    }
}
