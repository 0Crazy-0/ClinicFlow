using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexesForFamilyMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FamilyMemberships_PatientId",
                table: "FamilyMemberships"
            );

            migrationBuilder.DropIndex(
                name: "IX_FamilyMemberships_UserId",
                table: "FamilyMemberships"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberships_PatientId_Status_Role",
                table: "FamilyMemberships",
                columns: new[] { "PatientId", "Status", "Role" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberships_UserId_PatientId_Status_Role",
                table: "FamilyMemberships",
                columns: new[] { "UserId", "PatientId", "Status", "Role" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberships_UserId_Status_Role",
                table: "FamilyMemberships",
                columns: new[] { "UserId", "Status", "Role" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FamilyMemberships_PatientId_Status_Role",
                table: "FamilyMemberships"
            );

            migrationBuilder.DropIndex(
                name: "IX_FamilyMemberships_UserId_PatientId_Status_Role",
                table: "FamilyMemberships"
            );

            migrationBuilder.DropIndex(
                name: "IX_FamilyMemberships_UserId_Status_Role",
                table: "FamilyMemberships"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberships_PatientId",
                table: "FamilyMemberships",
                column: "PatientId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberships_UserId",
                table: "FamilyMemberships",
                column: "UserId"
            );
        }
    }
}
