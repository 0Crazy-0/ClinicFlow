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

    public ConsultationRoom ConsultationRoom { get; private set; } = null!;

    // EF Core constructor
    private Doctor() { }

    private Doctor(
        Guid userId,
        MedicalLicenseNumber licenseNumber,
        Guid medicalSpecialtyId,
        string biography,
        ConsultationRoom consultationRoom
    )
    {
        UserId = userId;
        LicenseNumber = licenseNumber;
        MedicalSpecialtyId = medicalSpecialtyId;
        Biography = biography;
        ConsultationRoom = consultationRoom;
    }

    public static Doctor Create(
        Guid userId,
        MedicalLicenseNumber licenseNumber,
        Guid medicalSpecialtyId,
        string biography,
        ConsultationRoom consultationRoom
    )
    {
        if (userId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (medicalSpecialtyId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        return new Doctor(userId, licenseNumber, medicalSpecialtyId, biography, consultationRoom);
    }

    public void UpdateProfile(string biography, ConsultationRoom consultationRoom)
    {
        Biography = biography;
        ConsultationRoom = consultationRoom;
    }
}
