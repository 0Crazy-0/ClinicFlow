using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Args.NoShow;

/// <summary>
/// Encapsulates the arguments required to mark an appointment as a no-show.
/// </summary>
public record AppointmentNoShowArgs
{
    public UserRole InitiatorRole { get; init; }
    public Guid? InitiatorDoctorId { get; init; }
}
