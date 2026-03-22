using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a physician registered in the clinic, linked to a user account and medical specialty.
/// </summary>
public class Doctor : BaseEntity
{
    public Guid UserId { get; init; }

    public Guid MedicalSpecialtyId { get; init; }

    public MedicalLicenseNumber LicenseNumber { get; private set; } = null!;

    public string Biography { get; private set; } = string.Empty;

    public int ConsultationRoomNumber { get; private set; }

    // EF Core constructor
    private Doctor() { }

    private Doctor(
        Guid userId,
        MedicalLicenseNumber licenseNumber,
        Guid medicalSpecialtyId,
        string biography,
        int consultationRoomNumber
    )
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
    internal static Doctor Create(
        Guid userId,
        MedicalLicenseNumber licenseNumber,
        Guid medicalSpecialtyId,
        string biography,
        int consultationRoomNumber
    )
    {
        if (userId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (medicalSpecialtyId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (consultationRoomNumber <= 0)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        return new Doctor(
            userId,
            licenseNumber,
            medicalSpecialtyId,
            biography,
            consultationRoomNumber
        );
    }
}
