using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class PatientPenaltyConfiguration : IEntityTypeConfiguration<PatientPenalty>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PatientPenalty> builder)
    {
        builder.Property(p => p.Type).HasConversion<string>();

        builder
            .HasOne<Patient>()
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.PatientId, p.SequenceNumber }).IsDescending(false, true);
        builder.HasIndex(p => new
        {
            p.IsRemoved,
            p.Type,
            p.BlockedUntil,
            p.SequenceNumber,
        });

        builder
            .HasIndex(p => new
            {
                p.IsRemoved,
                p.Type,
                p.SequenceNumber,
            })
            .IsDescending(false, false, true);
    }
}
