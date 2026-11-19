namespace ClinicFlow.Application.Interfaces;

/// <summary>
/// Computes the keyed HMAC of a patient portal activation code.
/// </summary>
public interface IActivationCodeHasher
{
    string ComputeHash(Guid userId, string activationCode);
}
