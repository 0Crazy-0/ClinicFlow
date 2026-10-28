using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitPersonNameColumnsAndAddPatientSearchName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Patients",
                newName: "FullName_FirstSurname"
            );

            migrationBuilder.RenameColumn(
                name: "EmergencyContactName",
                table: "Patients",
                newName: "FullName_FirstName"
            );

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Doctors",
                newName: "FullName_FirstSurname"
            );

            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactFirstName",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactFirstSurname",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactMiddleName",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "FullName_MiddleName",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "SearchName",
                table: "Patients",
                type: "character varying(201)",
                maxLength: 201,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "FullName_FirstName",
                table: "Doctors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "FullName_MiddleName",
                table: "Doctors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true
            );

            migrationBuilder
                .CreateIndex(
                    name: "IX_Patients_SearchName",
                    table: "Patients",
                    column: "SearchName"
                )
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Patients_SearchName", table: "Patients");

            migrationBuilder.DropColumn(name: "EmergencyContactFirstName", table: "Patients");

            migrationBuilder.DropColumn(name: "EmergencyContactFirstSurname", table: "Patients");

            migrationBuilder.DropColumn(name: "EmergencyContactMiddleName", table: "Patients");

            migrationBuilder.DropColumn(name: "FullName_MiddleName", table: "Patients");

            migrationBuilder.DropColumn(name: "SearchName", table: "Patients");

            migrationBuilder.DropColumn(name: "FullName_FirstName", table: "Doctors");

            migrationBuilder.DropColumn(name: "FullName_MiddleName", table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "FullName_FirstSurname",
                table: "Patients",
                newName: "FullName"
            );

            migrationBuilder.RenameColumn(
                name: "FullName_FirstName",
                table: "Patients",
                newName: "EmergencyContactName"
            );

            migrationBuilder.RenameColumn(
                name: "FullName_FirstSurname",
                table: "Doctors",
                newName: "FullName"
            );

            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
