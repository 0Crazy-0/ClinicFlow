using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required to mark an appointment as a no-show.
/// </summary>
public class AppointmentNoShowContext
{
    public UserRole InitiatorRole { get; init; }
    public Guid? InitiatorDoctorId { get; init; }
}
