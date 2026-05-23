using ClinicFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicFlow.Infrastructure.Persistence.Configurations;

public sealed class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.Property(s => s.DayOfWeek).HasConversion<string>();
        builder.OwnsOne(
            s => s.TimeRange,
            range =>
            {
                range.Property(r => r.Start).HasColumnName(ColumnNames.Schedule.StartTime);
                range.Property(r => r.End).HasColumnName(ColumnNames.Schedule.EndTime);
            }
        );

        builder
            .HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
