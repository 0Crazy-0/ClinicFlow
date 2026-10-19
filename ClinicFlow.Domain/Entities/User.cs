using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a user account in the system, responsible for authentication and access control.
/// </summary>
public class User : BaseEntity
{
    public const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public UserRole Role { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public EmailAddress Email { get; private set; } = null!;

    // Stryker disable once String
    public string PasswordHash { get; private set; } = string.Empty;

    public PhoneNumber Phone { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTime? LockoutEnd { get; private set; }

    // EF Core constructor
    private User()
    {
        IsActive = true;
        FailedLoginAttempts = 0;
    }

    private User(EmailAddress email, string passwordHash, PhoneNumber phoneNumber, UserRole role)
        : this()
    {
        Email = email;
        PasswordHash = passwordHash;
        Phone = phoneNumber;
        Role = role;
    }

    public static User Create(
        EmailAddress email,
        string passwordHash,
        PhoneNumber phoneNumber,
        UserRole role
    )
    {
        Guard.NotNullOrWhiteSpace(passwordHash);

        return new User(email, passwordHash, phoneNumber, role);
    }

    internal void RecordLogin(DateTime loginTime)
    {
        if (!IsActive)
            throw new BusinessRuleValidationException(DomainErrors.User.AccountInactive);

        if (LockoutEnd.HasValue && LockoutEnd > loginTime)
            throw new BusinessRuleValidationException(DomainErrors.User.AccountLockedOut);

        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginAt = loginTime;
    }

    internal void RecordFailedLogin(DateTime referenceTime)
    {
        Guard.NotDefault(referenceTime);

        if (!IsActive)
            throw new BusinessRuleValidationException(DomainErrors.User.AccountInactive);

        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
            LockoutEnd = referenceTime.Add(LockoutDuration);
    }

    public void ChangePassword(string newPasswordHash)
    {
        Guard.NotNullOrWhiteSpace(newPasswordHash);

        PasswordHash = newPasswordHash;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleValidationException(DomainErrors.User.AlreadyInactive);

        IsActive = false;
    }

    public void Reactivate()
    {
        if (IsActive)
            throw new BusinessRuleValidationException(DomainErrors.User.AlreadyActive);

        IsActive = true;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }
}
