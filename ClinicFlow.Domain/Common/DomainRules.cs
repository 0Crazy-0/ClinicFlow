namespace ClinicFlow.Domain.Common;

/// <summary>
/// Centralizes cross-cutting business rules shared by multiple subdomains.
/// </summary>
public static class DomainRules
{
    /// <summary>
    /// Age in years at which a patient is considered an adult for family
    /// management and clinical consent purposes.
    /// </summary>
    /// <remarks>
    /// Statutory ages, such as the age of majority governing protected care
    /// categories, derive from law and stay independent of this rule.
    /// </remarks>
    public const int AdultAge = 18;
}
