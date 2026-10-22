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
            .HasMaxLength(PersonName.MaximumLength);

        builder.Property(p => p.SexAtBirth).HasConversion<string>();

        builder
            .Property(p => p.BloodGroup)
            .HasConversion(bt => bt.Value, val => BloodType.Create(val))
            .HasMaxLength(5);

        builder.OwnsOne(
            p => p.Contact,
            contact =>
            {
                contact
                    .Property(c => c.Name)
                    .HasConversion(name => name.FullName, val => PersonName.Create(val))
                    .HasColumnName(ColumnNames.Patient.EmergencyContactName)
                    .HasMaxLength(PersonName.MaximumLength);
                contact
                    .Property(c => c.Phone)
                    .HasConversion(phone => phone.Value, val => PhoneNumber.Create(val))
                    .HasColumnName(ColumnNames.Patient.EmergencyContactPhone)
                    .HasMaxLength(PhoneNumber.MaximumLength);
            }
        );

        builder.OwnsOne(
            p => p.HomeAddress,
            address =>
            {
                address
                    .Property(a => a.Line)
                    .HasColumnName(ColumnNames.Patient.HomeAddressLine)
                    .HasMaxLength(Address.LineMaximumLength);
                address
                    .Property(a => a.State)
                    .HasConversion<string>()
                    .HasColumnName(ColumnNames.Patient.HomeAddressState);
                address
                    .Property(a => a.PostalCode)
                    .HasColumnName(ColumnNames.Patient.HomeAddressPostalCode)
                    .HasMaxLength(Address.PostalCodeMaximumLength);
                address
                    .Property(a => a.Country)
                    .HasConversion<string>()
                    .HasColumnName(ColumnNames.Patient.HomeAddressCountry);
            }
        );
    }
}
