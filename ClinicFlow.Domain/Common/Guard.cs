using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Common;

/// <summary>
/// Centralizes identical input validations shared by entities and value objects.
/// </summary>
public static class Guard
{
    public static void NotEmpty(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
    }

    public static void NotDefault(DateTime value)
    {
        if (value == default)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
    }

    public static void NotNullOrWhiteSpace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
    }

    /// <remarks>
    /// Only entities and value objects use this guard. Domain services report caller errors with ArgumentNullException instead.
    /// </remarks>
    public static void NotNull<T>(T? value)
        where T : class
    {
        if (value is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
    }

    public static void IsDefined<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new DomainValidationException(DomainErrors.Validation.InvalidEnumValue);
    }

    /// <summary>
    /// Ensures the optional value, when present, is a defined member of its enumeration.
    /// </summary>
    public static void IsDefined<TEnum>(TEnum? value)
        where TEnum : struct, Enum
    {
        if (value is not null && !Enum.IsDefined(value.Value))
            throw new DomainValidationException(DomainErrors.Validation.InvalidEnumValue);
    }
}
