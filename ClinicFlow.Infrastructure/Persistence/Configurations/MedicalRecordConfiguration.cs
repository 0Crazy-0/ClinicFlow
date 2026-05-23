using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder
            .HasMany<DynamicClinicalDetail>("_clinicalDetails")
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
