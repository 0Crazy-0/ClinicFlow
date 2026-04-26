using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentReschedulingNotAllowedException(string errorCode)
    : DomainException(errorCode) { }
