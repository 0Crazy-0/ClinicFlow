using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Services;

/// <summary>
/// Abstraction for phone number verification via external providers.
/// </summary>
public interface IPhoneVerificationService
{
    Task SendVerificationCodeAsync(PhoneNumber phoneNumber, CancellationToken ct = default);

    Task<bool> VerifyCodeAsync(
        PhoneNumber phoneNumber,
        string code,
        CancellationToken ct = default
    );
}
