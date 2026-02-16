using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.Entities;

public class User : BaseEntity
{
    public Guid? DoctorId { get; init; }
    public Guid? PatientId { get; init; }
    public UserRoleEnum Role { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    // EF Core constructor
    private User()
    {
        IsActive = true;
    }

    private User(string email, string passwordHash, string fullName, string phoneNumber, UserRoleEnum role, Guid? doctorId = null, Guid? patientId = null) : this()
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
    internal static User Create(string email, string passwordHash, string fullName, string phoneNumber, UserRoleEnum role, Guid? doctorId = null, Guid? patientId = null)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new BusinessRuleValidationException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new BusinessRuleValidationException("Full name cannot be empty.");
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new BusinessRuleValidationException("Phone number cannot be empty.");

        return new User(email, passwordHash, fullName, phoneNumber, role, doctorId, patientId);
    }
}