using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConsentGrantConfiguration
    : IEntityTypeConfiguration<AppointmentConsentGrant>
{
    public void Configure(EntityTypeBuilder<AppointmentConsentGrant> builder)
    {
        builder.Property(g => g.Scope).HasConversion<string>();
        builder.Property(g => g.SignedAt).HasColumnType("timestamp without time zone");
        builder.Property(g => g.ExpiresAt).HasColumnType("timestamp without time zone");
        builder.Property(g => g.RevokedAt).HasColumnType("timestamp without time zone");

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
            .HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(g => g.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.SignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => new
        {
            g.PatientId,
            g.RecipientMembershipId,
            g.Scope,
        });
    }
}
