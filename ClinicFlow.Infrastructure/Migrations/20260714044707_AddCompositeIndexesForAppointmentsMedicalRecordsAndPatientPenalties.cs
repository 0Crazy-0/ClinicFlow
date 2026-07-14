using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexesForAppointmentsMedicalRecordsAndPatientPenalties
        : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PatientPenalties_PatientId",
                table: "PatientPenalties"
            );

            migrationBuilder.DropIndex(name: "IX_MedicalRecords_DoctorId", table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PatientPenalties_IsRemoved_Type_BlockedUntil_SequenceNumber",
                table: "PatientPenalties",
                columns: new[] { "IsRemoved", "Type", "BlockedUntil", "SequenceNumber" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PatientPenalties_IsRemoved_Type_SequenceNumber",
                table: "PatientPenalties",
                columns: new[] { "IsRemoved", "Type", "SequenceNumber" },
                descending: new[] { false, false, true }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PatientPenalties_PatientId_SequenceNumber",
                table: "PatientPenalties",
                columns: new[] { "PatientId", "SequenceNumber" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_DoctorId_SequenceNumber",
                table: "MedicalRecords",
                columns: new[] { "DoctorId", "SequenceNumber" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId_SequenceNumber",
                table: "MedicalRecords",
                columns: new[] { "PatientId", "SequenceNumber" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_ScheduledDate_StartTime_SequenceNumber",
                table: "Appointments",
                columns: new[] { "DoctorId", "ScheduledDate", "StartTime", "SequenceNumber" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_ScheduledDate_StartTime_SequenceNumber",
                table: "Appointments",
                columns: new[] { "PatientId", "ScheduledDate", "StartTime", "SequenceNumber" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ScheduledDate_StartTime_SequenceNumber",
                table: "Appointments",
                columns: new[] { "ScheduledDate", "StartTime", "SequenceNumber" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PatientPenalties_IsRemoved_Type_BlockedUntil_SequenceNumber",
                table: "PatientPenalties"
            );

            migrationBuilder.DropIndex(
                name: "IX_PatientPenalties_IsRemoved_Type_SequenceNumber",
                table: "PatientPenalties"
            );

            migrationBuilder.DropIndex(
                name: "IX_PatientPenalties_PatientId_SequenceNumber",
                table: "PatientPenalties"
            );

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_DoctorId_SequenceNumber",
                table: "MedicalRecords"
            );

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_PatientId_SequenceNumber",
                table: "MedicalRecords"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PatientPenalties_PatientId",
                table: "PatientPenalties",
                column: "PatientId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_DoctorId",
                table: "MedicalRecords",
                column: "DoctorId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords",
                column: "PatientId"
            );

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId_ScheduledDate_StartTime_SequenceNumber",
                table: "Appointments"
            );

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId_ScheduledDate_StartTime_SequenceNumber",
                table: "Appointments"
            );

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ScheduledDate_StartTime_SequenceNumber",
                table: "Appointments"
            );
        }
    }
}
