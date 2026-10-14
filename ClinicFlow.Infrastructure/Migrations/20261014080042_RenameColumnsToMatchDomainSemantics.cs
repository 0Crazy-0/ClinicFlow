using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsToMatchDomainSemantics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "PhoneNumber", table: "Users", newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "DayOfWeek",
                table: "Schedules",
                newName: "Weekday"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_DoctorId_DayOfWeek",
                table: "Schedules",
                newName: "IX_Schedules_DoctorId_Weekday"
            );

            migrationBuilder.RenameColumn(
                name: "BloodType",
                table: "Patients",
                newName: "BloodGroup"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Phone", table: "Users", newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "Weekday",
                table: "Schedules",
                newName: "DayOfWeek"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_DoctorId_Weekday",
                table: "Schedules",
                newName: "IX_Schedules_DoctorId_DayOfWeek"
            );

            migrationBuilder.RenameColumn(
                name: "BloodGroup",
                table: "Patients",
                newName: "BloodType"
            );
        }
    }
}
