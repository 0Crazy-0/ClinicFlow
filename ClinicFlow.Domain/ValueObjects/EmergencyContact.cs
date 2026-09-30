using ClinicFlow.Domain.Common;

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

    internal static EmergencyContact Create(string name, string phoneNumber)
    {
        var nameVo = PersonName.Create(name);
        var phoneVo = PhoneNumber.Create(phoneNumber);

        return Create(nameVo, phoneVo);
    }

    internal static EmergencyContact Create(PersonName name, PhoneNumber phoneNumber)
    {
        Guard.NotNull(name);
        Guard.NotNull(phoneNumber);

        return new EmergencyContact(name, phoneNumber);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({PhoneNumber})";
}
