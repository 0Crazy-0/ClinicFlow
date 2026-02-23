using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a physician registered in the clinic, linked to a user account and medical specialty.
/// </summary>
public class Doctor : BaseEntity
{
    /// <summary>
    /// Identifier of the associated user account.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Identifier of the doctor's medical specialty.
    /// </summary>
    public Guid MedicalSpecialtyId { get; init; }

    /// <summary>
    /// The doctor's official medical license number.
    /// </summary>
    public MedicalLicenseNumber LicenseNumber { get; private set; } = null!;

    /// <summary>
    /// Short biographical text about the doctor.
    /// </summary>
    public string Biography { get; private set; } = string.Empty;

    /// <summary>
    /// Room number assigned for consultations.
    /// </summary>
    public int ConsultationRoomNumber { get; private set; }

    // EF Core constructor
    private Doctor() { }

    private Doctor(Guid userId, MedicalLicenseNumber licenseNumber, Guid medicalSpecialtyId, string biography, int consultationRoomNumber)
    {
        UserId = userId;
        LicenseNumber = licenseNumber;
        MedicalSpecialtyId = medicalSpecialtyId;
        Biography = biography;
        ConsultationRoomNumber = consultationRoomNumber;
    }

    /// <summary>
    /// Creates a new doctor entity.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the user ID or specialty ID is empty, or the room number is not positive.</exception>
    internal static Doctor Create(Guid userId, MedicalLicenseNumber licenseNumber, Guid medicalSpecialtyId, string biography, int consultationRoomNumber)
    {
        if (userId == Guid.Empty) throw new DomainValidationException("User ID cannot be empty.");
        if (medicalSpecialtyId == Guid.Empty) throw new DomainValidationException("Medical specialty ID cannot be empty.");
        if (consultationRoomNumber <= 0) throw new DomainValidationException("Consultation room number must be positive.");

        return new Doctor(userId, licenseNumber, medicalSpecialtyId, biography, consultationRoomNumber);
    }
}
