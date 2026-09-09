using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRecordGuardianAccessFieldsAndRenameProtectedCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProtectedCategory",
                table: "MedicalRecords",
                newName: "ProtectedCareCategory"
            );

            migrationBuilder.AddColumn<bool>(
                name: "GuardianInitiatedTreatment",
                table: "MedicalRecords",
                type: "boolean",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "GuardianInvolvementDeemedAppropriate",
                table: "MedicalRecords",
                type: "boolean",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuardianInitiatedTreatment",
                table: "MedicalRecords"
            );

            migrationBuilder.DropColumn(
                name: "GuardianInvolvementDeemedAppropriate",
                table: "MedicalRecords"
            );

            migrationBuilder.RenameColumn(
                name: "ProtectedCareCategory",
                table: "MedicalRecords",
                newName: "ProtectedCategory"
            );
        }
    }
}
