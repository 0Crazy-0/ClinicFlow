namespace ClinicFlow.Application.Interfaces;

/// <summary>
/// Computes the keyed HMAC of a patient's SSN last-4 digits.
/// </summary>
public interface ISsnHasher
{
    string ComputeHash(Guid patientId, string last4);
}
