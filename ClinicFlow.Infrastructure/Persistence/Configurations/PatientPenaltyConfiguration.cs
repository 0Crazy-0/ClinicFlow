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
    }
}
