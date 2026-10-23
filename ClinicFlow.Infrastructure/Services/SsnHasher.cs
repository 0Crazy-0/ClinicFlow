using System.Security.Cryptography;
using System.Text;
using ClinicFlow.Application.Interfaces;

namespace ClinicFlow.Infrastructure.Services;

public sealed class SsnHasher(string ssnHashKey) : ISsnHasher
{
    public const int MinKeyLengthBytes = 32;

    public const string MissingKeyMessage =
        "Security configuration is missing. Ensure 'Security:SsnHashKey' is set to a Base64 key of at least 256 bits.";

    public const string InvalidKeyMessage =
        "Security configuration is invalid. Ensure 'Security:SsnHashKey' is a valid Base64 string.";

    public const string TooShortKeyMessage =
        "Security configuration is too weak. The decoded 'Security:SsnHashKey' must be at least 256 bits (32 bytes).";

    private const string InvalidLast4Message = "SSN last-4 must be exactly 4 digits.";

    private readonly byte[] _key = DecodeKey(ssnHashKey);

    private static byte[] DecodeKey(string ssnHashKey)
    {
        if (string.IsNullOrWhiteSpace(ssnHashKey))
            throw new InvalidOperationException(MissingKeyMessage);

        byte[] decoded;

        try
        {
            decoded = Convert.FromBase64String(ssnHashKey);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(InvalidKeyMessage);
        }

        if (decoded.Length < MinKeyLengthBytes)
            throw new InvalidOperationException(TooShortKeyMessage);

        return decoded;
    }

    public string ComputeHash(Guid patientId, string last4)
    {
        // Defensive guard: invalid last4 here is a programmer bug; business validation
        // with its error code lives in command validators (future registration refactor).
        if (last4 is null || last4.Length != 4 || last4.Any(c => !char.IsAsciiDigit(c)))
            throw new ArgumentException(InvalidLast4Message, nameof(last4));

        using var hmac = new HMACSHA256(_key);
        var digest = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{patientId:N}:{last4}"));

        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}
