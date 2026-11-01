using System.Security.Cryptography;

namespace ClinicFlow.Domain.Common;

/// <summary>
/// Generates zero-padded, eight-digit activation codes suitable for a patient to read out loud.
/// </summary>
public static class ActivationCodeGenerator
{
    public static string Generate() => RandomNumberGenerator.GetInt32(100_000_000).ToString("D8");
}
