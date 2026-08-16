using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserFieldsAndSoftDeleteFromPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Patients_Users_UserId", table: "Patients");

            migrationBuilder.DropIndex(name: "IX_Patients_UserId", table: "Patients");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "Patients");

            migrationBuilder.DropColumn(name: "OriginalUserId", table: "Patients");

            migrationBuilder.DropColumn(name: "RelationshipToUser", table: "Patients");

            migrationBuilder.DropColumn(name: "UserId", table: "Patients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Patients",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalUserId",
                table: "Patients",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "RelationshipToUser",
                table: "Patients",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Patients",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserId",
                table: "Patients",
                column: "UserId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Users_UserId",
                table: "Patients",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
