namespace ClinicFlow.Domain.Exceptions.Base;

public class BusinessRuleValidationException(string errorCode) : DomainException(errorCode) { }
