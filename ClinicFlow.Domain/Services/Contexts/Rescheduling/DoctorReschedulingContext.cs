using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Rescheduling;

public sealed record class DoctorReschedulingContext
{
    public required Doctor InitiatorDoctor { get; init; }
    public required Schedule DoctorSchedule { get; init; }
    public required TimeZoneInfo ClinicTimeZone { get; init; }
}
