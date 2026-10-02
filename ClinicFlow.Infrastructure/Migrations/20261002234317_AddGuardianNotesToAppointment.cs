using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuardianNotesToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuardianNotes",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<Guid>(
                name: "GuardianNotesAuthorUserId",
                table: "Appointments",
                type: "uuid",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "GuardianNotes", table: "Appointments");

            migrationBuilder.DropColumn(name: "GuardianNotesAuthorUserId", table: "Appointments");
        }
    }
}
