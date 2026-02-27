using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a system user with authentication credentials and an assigned role.
/// May optionally be linked to a <see cref="Doctor"/> or <see cref="Patient"/> profile.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Identifier of the linked doctor profile, if the user is a doctor.
    /// </summary>
    public Guid? DoctorId { get; init; }

    /// <summary>
    /// Identifier of the linked patient profile, if the user is a patient.
    /// </summary>
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

    /// <summary>
    /// Creates a new user entity.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the password hash is blank, or a linked profile ID is provided as <see cref="Guid.Empty"/>.</exception>
    internal static User Create(EmailAddress email, string passwordHash, PersonName fullName, PhoneNumber phoneNumber, UserRole role, Guid? doctorId = null, Guid? patientId = null)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainValidationException("Password hash cannot be empty.");
        if (doctorId.HasValue && doctorId.Value == Guid.Empty) throw new DomainValidationException("Doctor ID cannot be empty.");
        if (patientId.HasValue && patientId.Value == Guid.Empty) throw new DomainValidationException("Patient ID cannot be empty.");

        return new User(email, passwordHash, fullName, phoneNumber, role, doctorId, patientId);
    }
}