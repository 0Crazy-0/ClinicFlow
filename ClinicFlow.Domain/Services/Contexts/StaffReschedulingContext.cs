using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record class StaffReschedulingContext
{
    public required Schedule DoctorSchedule { get; init; }
    public required TimeZoneInfo ClinicTimeZone { get; init; }
}
