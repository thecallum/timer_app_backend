using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace timer_app.Migrations
{
    public partial class AddIndexToCalendarEventTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Event_Start_End",
                table: "CalendarEvents",
                columns: new[] { "UserId", "StartTime", "EndTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Event_Start_End",
                table: "CalendarEvents");
        }
    }
}
