using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

public class PatientAccessUnauthorizedException(string errorCode) : DomainException(errorCode) { }
