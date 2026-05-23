using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class MedicalSpecialtyConfiguration : IEntityTypeConfiguration<MedicalSpecialty>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MedicalSpecialty> builder)
    {
        builder
            .Property(s => s.TypicalDuration)
            .HasConversion(dur => dur.Minutes, val => EncounterDuration.FromMinutes(val));

        builder
            .Property(s => s.CancellationPolicy)
            .HasConversion(policy => policy.Hours, val => CancellationLimit.FromHours(val));
    }
}
