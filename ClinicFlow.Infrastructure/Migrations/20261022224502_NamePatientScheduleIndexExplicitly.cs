using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NamePatientScheduleIndexExplicitly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PatientId_ScheduledDate_StartTime_SequenceNumbe",
                table: "Appointments",
                newName: "IX_Appointments_PatientId_ScheduledDate_StartTime_SeqNum"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PatientId_ScheduledDate_StartTime_SeqNum",
                table: "Appointments",
                newName: "IX_Appointments_PatientId_ScheduledDate_StartTime_SequenceNumbe"
            );
        }
    }
}
