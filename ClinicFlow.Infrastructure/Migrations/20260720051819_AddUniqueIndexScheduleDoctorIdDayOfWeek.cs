using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexScheduleDoctorIdDayOfWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Schedules_DoctorId", table: "Schedules");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DoctorId_DayOfWeek",
                table: "Schedules",
                columns: new[] { "DoctorId", "DayOfWeek" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Schedules_DoctorId_DayOfWeek", table: "Schedules");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DoctorId",
                table: "Schedules",
                column: "DoctorId"
            );
        }
    }
}
