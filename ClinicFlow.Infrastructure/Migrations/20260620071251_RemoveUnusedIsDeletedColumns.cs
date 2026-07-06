using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedIsDeletedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsDeleted", table: "Users");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "Schedules");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "PatientPenalties");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "MedicalRecords");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "DynamicClinicalDetail");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PatientPenalties",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MedicalRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DynamicClinicalDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }
    }
}
