using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required to reassign an appointment to a new doctor.
/// </summary>
public sealed record class AppointmentReassignmentContext
{
    public Schedule? NewDoctorSchedule { get; init; }
}
