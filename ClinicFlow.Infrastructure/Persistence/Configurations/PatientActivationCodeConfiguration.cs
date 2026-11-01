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

        builder.Property(c => c.IssuedAt).HasColumnType("timestamp without time zone");
        builder.Property(c => c.ExpiresAt).HasColumnType("timestamp without time zone");
        builder.Property(c => c.ConsumedAt).HasColumnType("timestamp without time zone");
        builder.Property(c => c.SupersededAt).HasColumnType("timestamp without time zone");

        builder
            .HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("\"ConsumedAt\" IS NULL AND \"SupersededAt\" IS NULL");
    }
}
