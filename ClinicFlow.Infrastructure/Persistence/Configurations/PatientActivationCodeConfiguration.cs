using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class PatientActivationCodeConfiguration
    : IEntityTypeConfiguration<PatientActivationCode>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PatientActivationCode> builder)
    {
        builder.Property(c => c.CodeHash).HasMaxLength(64).IsUnicode(false);

        builder
            .HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("\"ConsumedAt\" IS NULL AND \"SupersededAt\" IS NULL");
    }
}
