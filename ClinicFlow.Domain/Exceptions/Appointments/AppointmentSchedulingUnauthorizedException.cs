using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentSchedulingUnauthorizedException(string errorCode)
    : DomainException(errorCode) { }
