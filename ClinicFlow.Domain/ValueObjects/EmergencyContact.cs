using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.ValueObjects;

public record EmergencyContact
{
    public PersonName Name { get; private set; } = null!;

    public PhoneNumber Phone { get; private set; } = null!;

    // EF Core cannot bind the nested PersonName through a constructor parameter,
    // so it uses this parameterless constructor and sets the properties through
    // their private setters.
    private EmergencyContact() { }

    private EmergencyContact(PersonName name, PhoneNumber phone)
    {
        Name = name;
        Phone = phone;
    }

    public static EmergencyContact Create(
        string firstName,
        string? middleName,
        string firstSurname,
        string phoneNumber
    )
    {
        Guard.NotNullOrWhiteSpace(firstName);

        if (middleName is not null)
            Guard.NotNullOrWhiteSpace(middleName);

        Guard.NotNullOrWhiteSpace(firstSurname);
        Guard.NotNullOrWhiteSpace(phoneNumber);

        var phoneVo = PhoneNumber.Create(phoneNumber);

        return Create(PersonName.Create(firstName, middleName, firstSurname), phoneVo);
    }

    internal static EmergencyContact Create(PersonName name, PhoneNumber phoneNumber)
    {
        Guard.NotNull(name);
        Guard.NotNull(phoneNumber);

        return new EmergencyContact(name, phoneNumber);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Phone})";
}
