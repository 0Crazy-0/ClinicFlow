using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

public class IncompleteProfileException(string errorCode) : DomainException(errorCode) { }
