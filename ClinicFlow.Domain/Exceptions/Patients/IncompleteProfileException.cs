using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

/// <summary>
/// Thrown when an operation requires a complete medical profile, but the patient's profile is incomplete.
/// </summary>
public class IncompleteProfileException(string errorCode) : DomainException(errorCode)
{
}
