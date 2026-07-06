using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class DynamicClinicalDetailConfiguration
    : IEntityTypeConfiguration<DynamicClinicalDetail>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DynamicClinicalDetail> builder)
    {
        builder.Property<Guid>("MedicalRecordId").IsRequired();
        builder.Property(d => d.JsonDataPayload).HasColumnType("jsonb");
    }
}
