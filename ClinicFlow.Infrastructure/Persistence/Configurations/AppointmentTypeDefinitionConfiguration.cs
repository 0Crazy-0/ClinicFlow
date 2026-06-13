using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class AppointmentTypeDefinitionConfiguration
    : IEntityTypeConfiguration<AppointmentTypeDefinition>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppointmentTypeDefinition> builder)
    {
        builder.ToTable(
            "AppointmentTypes",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_AppointmentTypes_AgePolicyMinimumAge_Range",
                    AgeRangeConstraint(ColumnNames.AppointmentTypeDefinition.AgePolicyMinimumAge)
                );
                table.HasCheckConstraint(
                    "CK_AppointmentTypes_AgePolicyMaximumAge_Range",
                    AgeRangeConstraint(ColumnNames.AppointmentTypeDefinition.AgePolicyMaximumAge)
                );
            }
        );

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

    private static string AgeRangeConstraint(string col) =>
        $"\"{col}\" IS NULL OR \"{col}\" BETWEEN {AgeEligibilityPolicy.MinimumAllowedAge} AND {AgeEligibilityPolicy.MaximumAllowedAge}";
}
