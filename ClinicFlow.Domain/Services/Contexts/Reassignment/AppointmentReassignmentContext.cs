using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Reassignment;

public sealed record class AppointmentReassignmentContext
{
    public required Schedule NewDoctorSchedule { get; init; }
    public required TimeZoneInfo ClinicTimeZone { get; init; }
}
