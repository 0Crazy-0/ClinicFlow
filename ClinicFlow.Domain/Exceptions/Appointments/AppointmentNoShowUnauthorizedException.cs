using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentNoShowUnauthorizedException(string errorCode)
    : DomainException(errorCode) { }
