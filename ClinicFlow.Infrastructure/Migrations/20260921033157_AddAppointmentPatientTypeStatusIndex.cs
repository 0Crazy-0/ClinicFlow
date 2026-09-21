using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentPatientTypeStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Appointments_PatientId", table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_AppointmentTypeId_Status",
                table: "Appointments",
                columns: new[] { "PatientId", "AppointmentTypeId", "Status" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId_AppointmentTypeId_Status",
                table: "Appointments"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId"
            );
        }
    }
}
