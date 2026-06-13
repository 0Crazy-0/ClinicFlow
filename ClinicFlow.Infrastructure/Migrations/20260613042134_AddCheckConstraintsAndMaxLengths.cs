using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraintsAndMaxLengths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256
            );

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200
            );

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactPhone",
                table: "Patients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactName",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Doctors",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Doctors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_Doctors_ConsultationRoomFloor_Range",
                table: "Doctors",
                sql: "\"ConsultationRoomFloor\" BETWEEN 1 AND 8"
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_Doctors_ConsultationRoomNumber_Range",
                table: "Doctors",
                sql: "\"ConsultationRoomNumber\" BETWEEN 1 AND 35"
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppointmentTypes_AgePolicyMaximumAge_Range",
                table: "AppointmentTypes",
                sql: "\"AgePolicyMaximumAge\" IS NULL OR \"AgePolicyMaximumAge\" BETWEEN 0 AND 120"
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppointmentTypes_AgePolicyMinimumAge_Range",
                table: "AppointmentTypes",
                sql: "\"AgePolicyMinimumAge\" IS NULL OR \"AgePolicyMinimumAge\" BETWEEN 0 AND 120"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Doctors_ConsultationRoomFloor_Range",
                table: "Doctors"
            );

            migrationBuilder.DropCheckConstraint(
                name: "CK_Doctors_ConsultationRoomNumber_Range",
                table: "Doctors"
            );

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppointmentTypes_AgePolicyMaximumAge_Range",
                table: "AppointmentTypes"
            );

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppointmentTypes_AgePolicyMinimumAge_Range",
                table: "AppointmentTypes"
            );

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254
            );

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Patients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100
            );

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactPhone",
                table: "Patients",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactName",
                table: "Patients",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100
            );

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Doctors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15
            );

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Doctors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100
            );
        }
    }
}
