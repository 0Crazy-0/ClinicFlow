using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class FamilyMembershipConfiguration : IEntityTypeConfiguration<FamilyMembership>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FamilyMembership> builder)
    {
        builder.Property(m => m.Role).HasConversion<string>();
        builder.Property(m => m.Status).HasConversion<string>();
        builder.Property(m => m.AccessLevel).HasConversion<string>();

        builder
            .Property(m => m.AllowedAppointmentCategories)
            .HasField("_allowedAppointmentCategories")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasOne<Patient>()
            .WithMany()
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
