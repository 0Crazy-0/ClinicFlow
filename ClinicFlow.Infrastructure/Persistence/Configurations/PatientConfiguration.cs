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
        builder.ComplexProperty(
            p => p.FullName,
            name =>
            {
                name.Property(n => n.FirstName).HasMaxLength(PersonName.MaximumLength);
                name.Property(n => n.MiddleName).HasMaxLength(PersonName.MaximumLength);
                name.Property(n => n.FirstSurname).HasMaxLength(PersonName.MaximumLength);
                name.Ignore(n => n.SearchName);
            }
        );

        builder.HasIndex(p => p.SearchName).HasMethod("gin").HasOperators("gin_trgm_ops");

        builder.Property(p => p.SearchName).HasMaxLength(PersonName.SearchNameMaximumLength);
        builder.Property(p => p.SexAtBirth).HasConversion<string>();
        builder.Property(p => p.SsnLast4Hash).HasMaxLength(64).IsUnicode(false);

        builder
            .Property(p => p.BloodGroup)
            .HasConversion(bt => bt.Value, val => BloodType.Create(val))
            .HasMaxLength(5);

        builder.ComplexProperty(
            p => p.Contact,
            contact =>
            {
                contact.ComplexProperty(
                    c => c.Name,
                    name =>
                    {
                        name.Property(n => n.FirstName)
                            .HasColumnName(ColumnNames.Patient.EmergencyContactFirstName)
                            .HasMaxLength(PersonName.MaximumLength);
                        name.Property(n => n.MiddleName)
                            .HasColumnName(ColumnNames.Patient.EmergencyContactMiddleName)
                            .HasMaxLength(PersonName.MaximumLength);
                        name.Property(n => n.FirstSurname)
                            .HasColumnName(ColumnNames.Patient.EmergencyContactFirstSurname)
                            .HasMaxLength(PersonName.MaximumLength);
                        name.Ignore(n => n.SearchName);
                    }
                );

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
