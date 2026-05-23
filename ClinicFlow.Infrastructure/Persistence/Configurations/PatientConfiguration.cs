using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder
            .Property(p => p.FullName)
            .HasConversion(name => name.FullName, val => PersonName.Create(val))
            .HasMaxLength(200);

        builder.Property(p => p.RelationshipToUser).HasConversion<string>();
        builder
            .Property(p => p.BloodType)
            .HasConversion(bt => bt.Value, val => BloodType.Create(val))
            .HasMaxLength(5);

        builder.OwnsOne(
            p => p.EmergencyContact,
            contact =>
            {
                contact
                    .Property(c => c.Name)
                    .HasConversion(name => name.FullName, val => PersonName.Create(val))
                    .HasColumnName(ColumnNames.Patient.EmergencyContactName);
                contact
                    .Property(c => c.PhoneNumber)
                    .HasConversion(phone => phone.Value, val => PhoneNumber.Create(val))
                    .HasColumnName(ColumnNames.Patient.EmergencyContactPhone);
            }
        );

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
