namespace ClinicFlow.Infrastructure.Services;

/// <summary>
/// Decodes the Base64 hashing keys shared by the infrastructure hashers.
/// </summary>
public static class HasherKeyDecoder
{
    public static byte[] Decode(
        string key,
        int minKeyLengthBytes,
        string missingMessage,
        string invalidMessage,
        string tooShortMessage
    )
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException(missingMessage);

        byte[] decoded;

        try
        {
            decoded = Convert.FromBase64String(key);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(invalidMessage);
        }

        if (decoded.Length < minKeyLengthBytes)
            throw new InvalidOperationException(tooShortMessage);

        return decoded;
    }
}
