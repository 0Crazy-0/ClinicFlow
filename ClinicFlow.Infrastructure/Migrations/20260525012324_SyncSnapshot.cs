using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<string>(
                    name: "FullName",
                    table: "Doctors",
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "character varying(200)",
                    oldMaxLength: 200
                )
                .Annotation("Relational:ColumnOrder", 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<string>(
                    name: "FullName",
                    table: "Doctors",
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "character varying(200)",
                    oldMaxLength: 200
                )
                .OldAnnotation("Relational:ColumnOrder", 2);
        }
    }
}
