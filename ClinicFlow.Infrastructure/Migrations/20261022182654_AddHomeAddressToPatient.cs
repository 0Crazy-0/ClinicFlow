using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeAddressToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomeAddressCountry",
                table: "Patients",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "HomeAddressLine",
                table: "Patients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "HomeAddressPostalCode",
                table: "Patients",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "HomeAddressState",
                table: "Patients",
                type: "text",
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "HomeAddressCountry", table: "Patients");

            migrationBuilder.DropColumn(name: "HomeAddressLine", table: "Patients");

            migrationBuilder.DropColumn(name: "HomeAddressPostalCode", table: "Patients");

            migrationBuilder.DropColumn(name: "HomeAddressState", table: "Patients");
        }
    }
}
