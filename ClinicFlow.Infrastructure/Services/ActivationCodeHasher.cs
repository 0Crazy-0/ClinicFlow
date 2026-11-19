using System.Security.Cryptography;
using System.Text;
using ClinicFlow.Application.Interfaces;

namespace ClinicFlow.Infrastructure.Services;

public sealed class ActivationCodeHasher(string activationCodeHashKey) : IActivationCodeHasher
{
    public const int MinKeyLengthBytes = 32;

    public const string MissingKeyMessage =
        "Security configuration is missing. Ensure 'Security:ActivationCodeHashKey' is set to a Base64 key of at least 256 bits.";

    public const string InvalidKeyMessage =
        "Security configuration is invalid. Ensure 'Security:ActivationCodeHashKey' is a valid Base64 string.";

    public const string TooShortKeyMessage =
        "Security configuration is too weak. The decoded 'Security:ActivationCodeHashKey' must be at least 256 bits (32 bytes).";

    private const int CodeLength = 8;

    private const string InvalidCodeMessage = "Activation code must be exactly 8 digits.";

    private readonly byte[] _key = HasherKeyDecoder.Decode(
        activationCodeHashKey,
        MinKeyLengthBytes,
        MissingKeyMessage,
        InvalidKeyMessage,
        TooShortKeyMessage
    );

    public string ComputeHash(Guid userId, string activationCode)
    {
        // Defensive guard: an invalid code here is a programmer bug; business validation
        // with its error code lives in the activation command validators.
        if (
            activationCode is null
            || activationCode.Length != CodeLength
            || activationCode.Any(c => !char.IsAsciiDigit(c))
        )
            throw new ArgumentException(InvalidCodeMessage, nameof(activationCode));

        using var hmac = new HMACSHA256(_key);
        var digest = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{userId:N}:{activationCode}"));

        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}
