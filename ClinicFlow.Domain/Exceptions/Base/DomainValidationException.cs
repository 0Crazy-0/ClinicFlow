namespace ClinicFlow.Domain.Exceptions.Base;

public class DomainValidationException(string errorCode) : DomainException(errorCode) { }
