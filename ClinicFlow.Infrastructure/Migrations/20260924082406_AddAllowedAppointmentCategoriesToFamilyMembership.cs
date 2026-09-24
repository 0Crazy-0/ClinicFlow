using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedAppointmentCategoriesToFamilyMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "AllowedAppointmentCategories",
                table: "FamilyMemberships",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedAppointmentCategories",
                table: "FamilyMemberships"
            );
        }
    }
}
