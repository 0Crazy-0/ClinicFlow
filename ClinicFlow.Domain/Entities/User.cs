using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

public class User : BaseEntity
{
    public Guid? DoctorId { get; init; }
    public Guid? PatientId { get; init; }
    public UserRole Role { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public EmailAddress Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public PersonName FullName { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public bool IsActive { get; private set; }

    // EF Core constructor
    private User()
    {
        IsActive = true;
    }

    private User(EmailAddress email, string passwordHash, PersonName fullName, PhoneNumber phoneNumber, UserRole role, Guid? doctorId = null, Guid? patientId = null) : this()
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Role = role;
        DoctorId = doctorId;
        PatientId = patientId;
    }

    // Factory Method
    internal static User Create(EmailAddress email, string passwordHash, PersonName fullName, PhoneNumber phoneNumber, UserRole role, Guid? doctorId = null, Guid? patientId = null)
    {
        return new User(email, passwordHash, fullName, phoneNumber, role, doctorId, patientId);
    }
}