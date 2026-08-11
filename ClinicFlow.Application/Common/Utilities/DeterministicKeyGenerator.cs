using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ClinicFlow.Application.Common.Utilities;

/// <summary>
/// Combines two values into a stable identifier that produces the same result for the same input.
/// </summary>
/// <remarks>
/// Useful as an advisory lock key when no natural entity Id exists yet to lock on.
/// </remarks>
public static class DeterministicKeyGenerator
{
    public static Guid FromComposite(string first, string second)
    {
        var normalized = string.Create(
            CultureInfo.InvariantCulture,
            $"{first.Length}:{first}{second.Length}:{second}"
        );
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return new Guid(hash[..16]);
    }
}
