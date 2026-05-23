using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class AppointmentTypeDefinitionConfiguration
    : IEntityTypeConfiguration<AppointmentTypeDefinition>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppointmentTypeDefinition> builder)
    {
        builder.Property(a => a.Category).HasConversion<string>();
        builder.OwnsOne(
            a => a.AgePolicy,
            policy =>
            {
                policy
                    .Property(p => p.MinimumAge)
                    .HasColumnName(ColumnNames.AppointmentTypeDefinition.AgePolicyMinimumAge);
                policy
                    .Property(p => p.MaximumAge)
                    .HasColumnName(ColumnNames.AppointmentTypeDefinition.AgePolicyMaximumAge);
                policy
                    .Property(p => p.RequiresLegalGuardian)
                    .HasColumnName(
                        ColumnNames.AppointmentTypeDefinition.AgePolicyRequiresLegalGuardian
                    );
            }
        );

        builder
            .Property(a => a.AllowedSpecialtyIds)
            .HasField("_allowedSpecialtyIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .Navigation(a => a.RequiredTemplates)
            .HasField("_requiredTemplates")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(a => a.RequiredTemplates)
            .WithMany()
            .UsingEntity(j => j.ToTable("AppointmentTypeDefinitionRequiredTemplates"));
    }
}
