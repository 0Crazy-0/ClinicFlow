using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261126005803_ConvertTimestampColumnsToTimestampWithTimeZone : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LockoutEnd",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LastLoginAt",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ActivatedAt",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "SupersededAt",
            table: "PatientActivationCodes",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "IssuedAt",
            table: "PatientActivationCodes",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone"
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ExpiresAt",
            table: "PatientActivationCodes",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone"
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ConsumedAt",
            table: "PatientActivationCodes",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "SignedAt",
            table: "MedicalRecordConsentGrants",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone"
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "RevokedAt",
            table: "MedicalRecordConsentGrants",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ExpiresAt",
            table: "MedicalRecordConsentGrants",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone"
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "StartedAt",
            table: "FamilyMemberships",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone"
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "EndedAt",
            table: "FamilyMemberships",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "StartedAt",
            table: "Appointments",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CompletedAt",
            table: "Appointments",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CancelledAt",
            table: "Appointments",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateOnly),
            oldType: "date",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "SignedAt",
            table: "AppointmentConsentGrants",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone"
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "RevokedAt",
            table: "AppointmentConsentGrants",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ExpiresAt",
            table: "AppointmentConsentGrants",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "LockoutEnd",
            table: "Users",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "LastLoginAt",
            table: "Users",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "ActivatedAt",
            table: "Users",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "SupersededAt",
            table: "PatientActivationCodes",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "IssuedAt",
            table: "PatientActivationCodes",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone"
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpiresAt",
            table: "PatientActivationCodes",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone"
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "ConsumedAt",
            table: "PatientActivationCodes",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "SignedAt",
            table: "MedicalRecordConsentGrants",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone"
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "RevokedAt",
            table: "MedicalRecordConsentGrants",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpiresAt",
            table: "MedicalRecordConsentGrants",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone"
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "StartedAt",
            table: "FamilyMemberships",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone"
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "EndedAt",
            table: "FamilyMemberships",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "StartedAt",
            table: "Appointments",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "CompletedAt",
            table: "Appointments",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateOnly>(
            name: "CancelledAt",
            table: "Appointments",
            type: "date",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "SignedAt",
            table: "AppointmentConsentGrants",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone"
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "RevokedAt",
            table: "AppointmentConsentGrants",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpiresAt",
            table: "AppointmentConsentGrants",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone"
        );
    }
}
