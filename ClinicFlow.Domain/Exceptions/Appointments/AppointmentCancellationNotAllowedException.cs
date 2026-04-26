using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentCancellationNotAllowedException(
    string errorCode,
    AppointmentStatus currentStatus
) : DomainException(errorCode)
{
    public AppointmentStatus CurrentStatus { get; } = currentStatus;
}
