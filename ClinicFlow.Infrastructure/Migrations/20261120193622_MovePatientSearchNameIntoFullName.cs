using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261120193622_MovePatientSearchNameIntoFullName : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Appointments_DoctorId", table: "Appointments");

        migrationBuilder.RenameColumn(
            name: "SearchName",
            table: "Patients",
            newName: "FullName_SearchName"
        );

        migrationBuilder.RenameIndex(
            name: "IX_Patients_SearchName",
            table: "Patients",
            newName: "IX_Patients_FullName_SearchName"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "FullName_SearchName",
            table: "Patients",
            newName: "SearchName"
        );

        migrationBuilder.RenameIndex(
            name: "IX_Patients_FullName_SearchName",
            table: "Patients",
            newName: "IX_Patients_SearchName"
        );

        migrationBuilder.CreateIndex(
            name: "IX_Appointments_DoctorId",
            table: "Appointments",
            column: "DoctorId"
        );
    }
}
