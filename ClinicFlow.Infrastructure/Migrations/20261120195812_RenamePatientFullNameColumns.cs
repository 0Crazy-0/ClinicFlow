using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261120195812_RenamePatientFullNameColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "FullName_SearchName",
            table: "Patients",
            newName: "SearchName"
        );

        migrationBuilder.RenameColumn(
            name: "FullName_MiddleName",
            table: "Patients",
            newName: "MiddleName"
        );

        migrationBuilder.RenameColumn(
            name: "FullName_FirstSurname",
            table: "Patients",
            newName: "FirstSurname"
        );

        migrationBuilder.RenameColumn(
            name: "FullName_FirstName",
            table: "Patients",
            newName: "FirstName"
        );

        migrationBuilder.RenameIndex(
            name: "IX_Patients_FullName_SearchName",
            table: "Patients",
            newName: "IX_Patients_SearchName"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "SearchName",
            table: "Patients",
            newName: "FullName_SearchName"
        );

        migrationBuilder.RenameColumn(
            name: "MiddleName",
            table: "Patients",
            newName: "FullName_MiddleName"
        );

        migrationBuilder.RenameColumn(
            name: "FirstSurname",
            table: "Patients",
            newName: "FullName_FirstSurname"
        );

        migrationBuilder.RenameColumn(
            name: "FirstName",
            table: "Patients",
            newName: "FullName_FirstName"
        );

        migrationBuilder.RenameIndex(
            name: "IX_Patients_SearchName",
            table: "Patients",
            newName: "IX_Patients_FullName_SearchName"
        );
    }
}
