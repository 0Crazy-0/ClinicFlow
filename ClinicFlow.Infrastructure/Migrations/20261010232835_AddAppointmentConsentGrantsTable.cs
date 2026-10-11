using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentConsentGrantsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentConsentGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    ProtectedCareCategory = table.Column<int>(type: "integer", nullable: true),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignedAt = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: false
                    ),
                    ExpiresAt = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: false
                    ),
                    RevokedAt = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: true
                    ),
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
                    table.PrimaryKey("PK_AppointmentConsentGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentConsentGrants_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_AppointmentConsentGrants_FamilyMemberships_RecipientMembers~",
                        column: x => x.RecipientMembershipId,
                        principalTable: "FamilyMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_AppointmentConsentGrants_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_AppointmentConsentGrants_Users_SignedByUserId",
                        column: x => x.SignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentConsentGrants_AppointmentId",
                table: "AppointmentConsentGrants",
                column: "AppointmentId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentConsentGrants_PatientId_RecipientMembershipId_Sc~",
                table: "AppointmentConsentGrants",
                columns: new[] { "PatientId", "RecipientMembershipId", "Scope" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentConsentGrants_RecipientMembershipId",
                table: "AppointmentConsentGrants",
                column: "RecipientMembershipId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentConsentGrants_SignedByUserId",
                table: "AppointmentConsentGrants",
                column: "SignedByUserId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AppointmentConsentGrants");
        }
    }
}
