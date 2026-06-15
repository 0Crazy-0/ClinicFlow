using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTimeSpanToTimeOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "Schedules",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval"
            );

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "Schedules",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval"
            );

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "Appointments",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval"
            );

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "Appointments",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "Schedules",
                type: "interval",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone"
            );

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndTime",
                table: "Schedules",
                type: "interval",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone"
            );

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "Appointments",
                type: "interval",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone"
            );

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndTime",
                table: "Appointments",
                type: "interval",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone"
            );
        }
    }
}
