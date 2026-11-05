using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a physician registered in the clinic, linked to a user account and medical specialty.
/// </summary>
public class Doctor : SoftDeletableEntity
{
    public Guid UserId { get; init; }

    public PersonName FullName { get; private set; } = null!;

    public Guid MedicalSpecialtyId { get; init; }

    public MedicalLicenseNumber LicenseNumber { get; private set; } = null!;

    // Stryker disable once String
    public string Biography { get; private set; } = string.Empty;

    public ConsultationRoom Room { get; private set; } = null!;

    // EF Core constructor
    private Doctor() { }

    private Doctor(
        Guid userId,
        PersonName fullName,
        MedicalLicenseNumber licenseNumber,
        Guid medicalSpecialtyId,
        string biography,
        ConsultationRoom consultationRoom
    )
    {
        UserId = userId;
        FullName = fullName;
        LicenseNumber = licenseNumber;
        MedicalSpecialtyId = medicalSpecialtyId;
        Biography = biography;
        Room = consultationRoom;
    }

    internal static Doctor Create(
        Guid userId,
        PersonName fullName,
        MedicalLicenseNumber licenseNumber,
        Guid medicalSpecialtyId,
        string biography,
        ConsultationRoom consultationRoom
    )
    {
        Guard.NotEmpty(userId);
        Guard.NotNull(fullName);
        Guard.NotNull(licenseNumber);
        Guard.NotEmpty(medicalSpecialtyId);
        Guard.NotNullOrWhiteSpace(biography);
        Guard.NotNull(consultationRoom);

        return new Doctor(
            userId,
            fullName,
            licenseNumber,
            medicalSpecialtyId,
            biography,
            consultationRoom
        );
    }

    public void UpdateProfile(string biography, ConsultationRoom consultationRoom)
    {
        Guard.NotNullOrWhiteSpace(biography);
        Guard.NotNull(consultationRoom);

        Biography = biography;
        Room = consultationRoom;
    }

    /// <remarks>
    /// Emitting <see cref="DoctorSuspendedEvent"/> triggers a downstream
    /// handler that automatically flags all of the doctor's future scheduled appointments as requiring reassignment.
    /// </remarks>
    public void Suspend()
    {
        if (IsDeleted)
            throw new BusinessRuleValidationException(DomainErrors.Doctor.AlreadySuspended);

        MarkAsDeleted();
        AddDomainEvent(new DoctorSuspendedEvent(Id));
    }

    /// <remarks>
    /// To update the profile information (biography or consultation room) of a suspended doctor,
    /// use <see cref="UpdateProfile"/> instead.
    /// </remarks>
    public void Reactivate()
    {
        if (!IsDeleted)
            throw new BusinessRuleValidationException(DomainErrors.Doctor.AlreadyActive);

        UndoDeletion();
    }
}
