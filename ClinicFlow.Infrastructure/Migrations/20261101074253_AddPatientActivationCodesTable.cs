using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientActivationCodesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientActivationCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(
                        type: "character varying(64)",
                        unicode: false,
                        maxLength: 64,
                        nullable: false
                    ),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedAt = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: false
                    ),
                    ExpiresAt = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: false
                    ),
                    ConsumedAt = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: true
                    ),
                    SupersededAt = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: true
                    ),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    SequenceNumber = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientActivationCodes", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PatientActivationCodes_UserId",
                table: "PatientActivationCodes",
                column: "UserId",
                unique: true,
                filter: "\"ConsumedAt\" IS NULL AND \"SupersededAt\" IS NULL"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PatientActivationCodes");
        }
    }
}
