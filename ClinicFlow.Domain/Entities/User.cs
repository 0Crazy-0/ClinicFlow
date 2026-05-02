using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

public class User : BaseEntity
{
    public UserRole Role { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public EmailAddress Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = string.Empty;

    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public bool IsPhoneVerified { get; private set; }

    // EF Core constructor
    private User()
    {
        IsActive = true;
        IsPhoneVerified = false;
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
}
