using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentOverlapExclusionConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.Sql(
                """
                ALTER TABLE "Appointments"
                ADD CONSTRAINT "EX_Appointments_NoOverlap"
                EXCLUDE USING gist (
                    "DoctorId" WITH =,
                    tsrange(
                        "ScheduledDate" + "StartTime",
                        "ScheduledDate" + "EndTime"
                    ) WITH &&
                )
                WHERE ("Status" NOT IN ('Cancelled', 'NoShow', 'LateCancellation'));
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """ALTER TABLE "Appointments" DROP CONSTRAINT "EX_Appointments_NoOverlap";"""
            );

            migrationBuilder
                .AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,");
        }
    }
}
