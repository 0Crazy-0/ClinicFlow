using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class ClinicalFormTemplateConfiguration
    : IEntityTypeConfiguration<ClinicalFormTemplate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClinicalFormTemplate> builder)
    {
        builder.Property(t => t.JsonSchemaDefinition).HasColumnType("jsonb");
        builder.HasIndex(t => t.Code).IsUnique();
    }
}
