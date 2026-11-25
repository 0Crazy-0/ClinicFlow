using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class MedicalRecordConsentGrantConfiguration
    : IEntityTypeConfiguration<MedicalRecordConsentGrant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MedicalRecordConsentGrant> builder)
    {
        builder
            .HasOne<Patient>()
            .WithMany()
            .HasForeignKey(g => g.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<FamilyMembership>()
            .WithMany()
            .HasForeignKey(g => g.RecipientMembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<MedicalRecord>()
            .WithMany()
            .HasForeignKey(g => g.MedicalRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.SignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => new { g.PatientId, g.SequenceNumber }).IsDescending(false, true);
        builder.HasIndex(g => new { g.MedicalRecordId, g.RecipientMembershipId });
        builder.HasIndex(g => new { g.PatientId, g.RecipientMembershipId });
    }
}
