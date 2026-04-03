using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a system user with authentication credentials and an assigned role.
/// </summary>
public class User : BaseEntity
{
    public UserRole Role { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public EmailAddress Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = string.Empty;

    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public bool IsActive { get; private set; }

    // EF Core constructor
    private User()
    {
        IsActive = true;
    }

    private User(EmailAddress email, string passwordHash, PhoneNumber phoneNumber, UserRole role)
        : this()
    {
        Email = email;
        PasswordHash = passwordHash;
        PhoneNumber = phoneNumber;
        Role = role;
    }

    /// <summary>
    /// Creates a new user entity.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the password hash is blank.</exception>
    public static User Create(
        EmailAddress email,
        string passwordHash,
        PhoneNumber phoneNumber,
        UserRole role
    )
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        return new User(email, passwordHash, phoneNumber, role);
    }
}
