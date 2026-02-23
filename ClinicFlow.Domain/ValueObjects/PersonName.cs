using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated person's full name.
/// </summary>
public record PersonName
{
    /// <summary>
    /// The trimmed full name string.
    /// </summary>
    public string FullName { get; }

    private PersonName(string fullName)
    {
        FullName = fullName;
    }
    
    /// <summary>
    /// Creates a <see cref="PersonName"/> after validating length constraints.
    /// </summary>
    /// <exception cref="BusinessRuleValidationException">Thrown when the value is empty or shorter than 2 characters.</exception>
    internal static PersonName Create(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new BusinessRuleValidationException("Name cannot be empty.");

        var trimmed = fullName.Trim();

        if (trimmed.Length < 2) throw new BusinessRuleValidationException("Name is too short.");

        return new PersonName(trimmed);
    }

    /// <inheritdoc/>
    public override string ToString() => FullName;

}
