using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record EmergencyContact
{
    public PersonName Name { get; }
    public PhoneNumber PhoneNumber { get; }

    private EmergencyContact(PersonName name, PhoneNumber phoneNumber)
    {
        Name = name;
        PhoneNumber = phoneNumber;
    }
    
    // Factory Methods
    internal static EmergencyContact Create(string name, string phoneNumber)
    {
        var nameVo = PersonName.Create(name);
        var phoneVo = PhoneNumber.Create(phoneNumber);

        return Create(nameVo, phoneVo);
    }

    internal static EmergencyContact Create(PersonName name, PhoneNumber phoneNumber)
    {
        if (name is null) throw new BusinessRuleValidationException("Emergency contact name cannot be null.");
        if (phoneNumber is null) throw new BusinessRuleValidationException("Emergency contact phone cannot be null.");

        return new EmergencyContact(name, phoneNumber);
    }

    public override string ToString() => $"{Name} ({PhoneNumber})";
}
