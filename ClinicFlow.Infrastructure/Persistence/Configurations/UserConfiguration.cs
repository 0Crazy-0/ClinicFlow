using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Role).HasConversion<string>();
        builder
            .Property(u => u.Email)
            .HasConversion(email => email.Value, val => EmailAddress.Create(val))
            .HasMaxLength(EmailAddress.MaximumLength);

        builder
            .Property(u => u.PhoneNumber)
            .HasConversion(phone => phone.Value, val => PhoneNumber.Create(val))
            .HasMaxLength(PhoneNumber.MaximumLength);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
