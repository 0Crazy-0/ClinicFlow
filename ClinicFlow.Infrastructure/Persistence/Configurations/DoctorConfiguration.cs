using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder
            .Property(d => d.LicenseNumber)
            .HasConversion(lic => lic.Value, val => MedicalLicenseNumber.Create(val))
            .HasMaxLength(50);

        builder.OwnsOne(
            d => d.ConsultationRoom,
            room =>
            {
                room.Property(r => r.Number)
                    .HasColumnName(ColumnNames.Doctor.ConsultationRoomNumber);
                room.Property(r => r.Name).HasColumnName(ColumnNames.Doctor.ConsultationRoomName);
                room.Property(r => r.Floor).HasColumnName(ColumnNames.Doctor.ConsultationRoomFloor);
            }
        );

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<MedicalSpecialty>()
            .WithMany()
            .HasForeignKey(d => d.MedicalSpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
