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
        builder.ToTable(
            "Doctors",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Doctors_ConsultationRoomNumber_Range",
                    BetweenConstraint(
                        ColumnNames.Doctor.ConsultationRoomNumber,
                        ConsultationRoom.MinimumNumber,
                        ConsultationRoom.MaximumNumber
                    )
                );
                table.HasCheckConstraint(
                    "CK_Doctors_ConsultationRoomFloor_Range",
                    BetweenConstraint(
                        ColumnNames.Doctor.ConsultationRoomFloor,
                        ConsultationRoom.MinimumFloor,
                        ConsultationRoom.MaximumFloor
                    )
                );
            }
        );

        builder
            .Property(d => d.FullName)
            .HasConversion(name => name.FullName, val => PersonName.Create(val))
            .HasMaxLength(PersonName.MaximumLength);

        builder
            .Property(d => d.LicenseNumber)
            .HasConversion(lic => lic.Value, val => MedicalLicenseNumber.Create(val))
            .HasMaxLength(MedicalLicenseNumber.MaximumLength);

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

    private static string BetweenConstraint(string col, int min, int max) =>
        $"\"{col}\" BETWEEN {min} AND {max}";
}
