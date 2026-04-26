using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentCancellationUnauthorizedException(string errorCode)
    : DomainException(errorCode) { }
