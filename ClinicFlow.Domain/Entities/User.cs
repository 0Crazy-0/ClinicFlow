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

    public string PasswordHash { get; private set; } = string.Empty;

    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public bool IsPhoneVerified { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTime? LockoutEnd { get; private set; }

    // EF Core constructor
    private User()
    {
        IsActive = true;
        IsPhoneVerified = false;
        FailedLoginAttempts = 0;
    }

    private User(EmailAddress email, string passwordHash, PhoneNumber phoneNumber, UserRole role)
        : this()
    {
        Email = email;
        PasswordHash = passwordHash;
        PhoneNumber = phoneNumber;
        Role = role;
    }

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

    /// <param name="isVerificationCodeValid">The result of the external verification check.</param>
    public void MarkPhoneAsVerified(bool isVerificationCodeValid)
    {
        if (!isVerificationCodeValid)
            throw new DomainValidationException(DomainErrors.User.InvalidVerificationCode);

        if (IsPhoneVerified)
            throw new DomainValidationException(DomainErrors.User.PhoneAlreadyVerified);

        IsPhoneVerified = true;
    }

    public void RecordLogin(DateTime loginTime)
    {
        if (!IsActive)
            throw new BusinessRuleValidationException(DomainErrors.User.AccountInactive);

        if (LockoutEnd.HasValue && LockoutEnd > loginTime)
            throw new BusinessRuleValidationException(DomainErrors.User.AccountLockedOut);

        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginAt = loginTime;
    }

    public void RecordFailedLogin(DateTime referenceTime)
    {
        if (!IsActive)
            throw new BusinessRuleValidationException(DomainErrors.User.AccountInactive);

        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
            LockoutEnd = referenceTime.Add(LockoutDuration);
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

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
