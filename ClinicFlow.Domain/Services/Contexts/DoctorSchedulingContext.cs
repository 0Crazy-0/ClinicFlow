using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record class DoctorSchedulingContext
{
    public required Doctor InitiatorDoctor { get; init; }
    public required Patient TargetPatient { get; init; }
    public required AppointmentTypeDefinition AppointmentType { get; init; }
    public required Schedule DoctorSchedule { get; init; }
    public required TimeZoneInfo ClinicTimeZone { get; init; }
}
