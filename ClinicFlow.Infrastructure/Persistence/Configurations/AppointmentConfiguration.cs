using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.Status).HasConversion<string>();
        builder.OwnsOne(
            a => a.TimeRange,
            range =>
            {
                range.Property(r => r.Start).HasColumnName(ColumnNames.Appointment.StartTime);
                range.Property(r => r.End).HasColumnName(ColumnNames.Appointment.EndTime);
            }
        );

        builder
            .HasOne<Patient>()
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<AppointmentTypeDefinition>()
            .WithMany()
            .HasForeignKey(a => a.AppointmentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
