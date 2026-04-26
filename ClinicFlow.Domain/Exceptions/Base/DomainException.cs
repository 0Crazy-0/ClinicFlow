namespace ClinicFlow.Domain.Exceptions.Base;

public abstract class DomainException(string errorCode) : Exception(errorCode) { }
