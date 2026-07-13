using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSequenceNumberToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "Users",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "Schedules",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "Patients",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "PatientPenalties",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "MedicalSpecialties",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "MedicalRecords",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "DynamicClinicalDetail",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "Doctors",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "ClinicalFormTemplates",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "AppointmentTypes",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AddColumn<long>(
                    name: "SequenceNumber",
                    table: "Appointments",
                    type: "bigint",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SequenceNumber", table: "Users");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "Schedules");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "Patients");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "PatientPenalties");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "MedicalSpecialties");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "MedicalRecords");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "DynamicClinicalDetail");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "Doctors");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "ClinicalFormTemplates");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "AppointmentTypes");

            migrationBuilder.DropColumn(name: "SequenceNumber", table: "Appointments");
        }
    }
}
