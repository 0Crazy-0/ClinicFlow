using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCheckedInAndCancelledAtToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "CheckedInAt",
                table: "Appointments",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CancelledAt",
                table: "Appointments",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckedInAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "CancelledAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true
            );
        }
    }
}
