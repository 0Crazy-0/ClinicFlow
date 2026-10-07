using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClinicFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRecordConsentGrantsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalRecordConsentGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicalRecordId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MedicalRecordConsentGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecordConsentGrants_FamilyMemberships_RecipientMembe~",
                        column: x => x.RecipientMembershipId,
                        principalTable: "FamilyMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_MedicalRecordConsentGrants_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_MedicalRecordConsentGrants_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_MedicalRecordConsentGrants_Users_SignedByUserId",
                        column: x => x.SignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordConsentGrants_MedicalRecordId_RecipientMembers~",
                table: "MedicalRecordConsentGrants",
                columns: new[] { "MedicalRecordId", "RecipientMembershipId" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordConsentGrants_PatientId_RecipientMembershipId",
                table: "MedicalRecordConsentGrants",
                columns: new[] { "PatientId", "RecipientMembershipId" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordConsentGrants_PatientId_SequenceNumber",
                table: "MedicalRecordConsentGrants",
                columns: new[] { "PatientId", "SequenceNumber" },
                descending: new[] { false, true }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordConsentGrants_RecipientMembershipId",
                table: "MedicalRecordConsentGrants",
                column: "RecipientMembershipId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordConsentGrants_SignedByUserId",
                table: "MedicalRecordConsentGrants",
                column: "SignedByUserId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MedicalRecordConsentGrants");
        }
    }
}
