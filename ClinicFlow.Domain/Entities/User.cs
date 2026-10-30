using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Users;
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

    public string? PasswordHash { get; private set; }

    public PhoneNumber Phone { get; private set; } = null!;

    public DateTime? ActivatedAt { get; private set; }

    public bool IsActive { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTime? LockoutEnd { get; private set; }

    // EF Core constructor
    private User()
    {
        IsActive = true;
        FailedLoginAttempts = 0;
    }

    private User(
        EmailAddress email,
        string? passwordHash,
        PhoneNumber phone,
        UserRole role,
        DateTime? activatedAt
    )
        : this()
    {
        Email = email;
        PasswordHash = passwordHash;
        Phone = phone;
        Role = role;
        ActivatedAt = activatedAt;
    }

    /// <summary>
    /// Creates a usable account for the staff roles provisioned by the clinic. Patient accounts are created
    /// without password through <see cref="CreatePending"/> and become usable through <see cref="Activate"/>.
    /// </summary>
    public static User Create(
        EmailAddress email,
        string passwordHash,
        PhoneNumber phoneNumber,
        UserRole role,
        DateTime referenceTime
    )
    {
        Guard.NotNullOrWhiteSpace(passwordHash);
        Guard.IsDefined(role);
        Guard.NotDefault(referenceTime);

        if (role is not (UserRole.Doctor or UserRole.Receptionist or UserRole.Admin))
            throw new DomainValidationException(DomainErrors.User.InvalidRole);

        return new User(email, passwordHash, phoneNumber, role, referenceTime);
    }

    /// <summary>
    /// Creates the pending user account for a patient registered by reception, without
    /// password until its holder activates the account with the issued activation code.
    /// </summary>
    public static User CreatePending(EmailAddress email, PhoneNumber phone) =>
        new(email, passwordHash: null, phone, UserRole.Patient, activatedAt: null);

    /// <summary>
    /// Sets the initial password of a pending account issued without one by reception. The first password can
    /// only be set this way; once activated, updates go through <see cref="ChangePassword"/>.
    /// </summary>
    /// <remarks>
    /// Usability is tied to a non-null <see cref="PasswordHash"/>, not to <see cref="ActivatedAt"/>: login and
    /// password change reject accounts whose hash is still null. Staff accounts provisioned with a password through
    /// <see cref="Create"/> are usable from creation and receive the creation time as their activation date. The first
    /// password of a pending account can only be set via <see cref="Activate"/>, never via
    /// <see cref="ChangePassword"/>.
    /// A pending account deactivated by the clinic cannot be activated until it is reactivated.
    /// </remarks>
    public void Activate(string passwordHash, DateTime referenceTime)
    {
        Guard.NotNullOrWhiteSpace(passwordHash);
        Guard.NotDefault(referenceTime);

        if (ActivatedAt is not null)
            throw new BusinessRuleValidationException(DomainErrors.User.AlreadyActivated);

        if (!IsActive)
            throw new BusinessRuleValidationException(DomainErrors.User.AccountInactive);

        PasswordHash = passwordHash;
        ActivatedAt = referenceTime;

        AddDomainEvent(new PatientAccountActivatedEvent(Id, ActivatedAt.Value));
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

        if (PasswordHash is null)
            throw new BusinessRuleValidationException(DomainErrors.User.PasswordChangeNotAllowed);

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
