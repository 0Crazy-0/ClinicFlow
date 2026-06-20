using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAppointmentTypeDurationMinutesToDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DurationMinutes", table: "AppointmentTypes");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "AppointmentTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Duration", table: "AppointmentTypes");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DurationMinutes",
                table: "AppointmentTypes",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0)
            );
        }
    }
}
