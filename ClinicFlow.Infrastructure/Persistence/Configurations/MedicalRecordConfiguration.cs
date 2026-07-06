using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder
            .Navigation(m => m.ClinicalDetails)
            .HasField("_clinicalDetails")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(m => m.ClinicalDetails)
            .WithOne()
            .HasForeignKey("MedicalRecordId")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Patient>()
            .WithMany()
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Appointment>()
            .WithOne()
            .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
