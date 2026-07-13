using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    // The AddColumn/DropColumn operations below target PostgreSQL's "xmin" system column,
    // which is filtered out by Npgsql's migrations SQL generator and produces no real DDL.
    // This migration only registers "xmin" as a concurrency token in EF Core's model.
    /// <inheritdoc />
    public partial class AddVersionConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Users",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Schedules",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Patients",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "PatientPenalties",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "MedicalSpecialties",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "MedicalRecords",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "DynamicClinicalDetail",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Doctors",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ClinicalFormTemplates",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AppointmentTypes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Appointments",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "xmin", table: "Users");

            migrationBuilder.DropColumn(name: "xmin", table: "Schedules");

            migrationBuilder.DropColumn(name: "xmin", table: "Patients");

            migrationBuilder.DropColumn(name: "xmin", table: "PatientPenalties");

            migrationBuilder.DropColumn(name: "xmin", table: "MedicalSpecialties");

            migrationBuilder.DropColumn(name: "xmin", table: "MedicalRecords");

            migrationBuilder.DropColumn(name: "xmin", table: "DynamicClinicalDetail");

            migrationBuilder.DropColumn(name: "xmin", table: "Doctors");

            migrationBuilder.DropColumn(name: "xmin", table: "ClinicalFormTemplates");

            migrationBuilder.DropColumn(name: "xmin", table: "AppointmentTypes");

            migrationBuilder.DropColumn(name: "xmin", table: "Appointments");
        }
    }
}
