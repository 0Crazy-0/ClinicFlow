using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

public class InvalidTimeRangeException(string errorCode) : DomainException(errorCode) { }
