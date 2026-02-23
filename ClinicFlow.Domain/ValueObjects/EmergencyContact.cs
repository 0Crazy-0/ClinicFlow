using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a patient's emergency contact, composed of a name and phone number.
/// </summary>
public record EmergencyContact
{
    /// <summary>
    /// Full name of the emergency contact person.
    /// </summary>
    public PersonName Name { get; }

    /// <summary>
    /// Phone number of the emergency contact person.
    /// </summary>
    public PhoneNumber PhoneNumber { get; }

    private EmergencyContact(PersonName name, PhoneNumber phoneNumber)
    {
        Name = name;
        PhoneNumber = phoneNumber;
    }
    
    /// <summary>
    /// Creates an <see cref="EmergencyContact"/> from raw string values, delegating validation to <see cref="PersonName"/> and <see cref="PhoneNumber"/>.
    /// </summary>
    internal static EmergencyContact Create(string name, string phoneNumber)
    {
        var nameVo = PersonName.Create(name);
        var phoneVo = PhoneNumber.Create(phoneNumber);

        return Create(nameVo, phoneVo);
    }

    /// <summary>
    /// Creates an <see cref="EmergencyContact"/> from pre-validated value objects.
    /// </summary>
    /// <exception cref="BusinessRuleValidationException">Thrown when either the name or phone number is null.</exception>
    internal static EmergencyContact Create(PersonName name, PhoneNumber phoneNumber)
    {
        if (name is null) throw new BusinessRuleValidationException("Emergency contact name cannot be null.");
        if (phoneNumber is null) throw new BusinessRuleValidationException("Emergency contact phone cannot be null.");

        return new EmergencyContact(name, phoneNumber);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({PhoneNumber})";
}
