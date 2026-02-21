using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

public class Doctor : BaseEntity
{
    public Guid UserId { get; init; }
    public Guid MedicalSpecialtyId { get; init; }
    public MedicalLicenseNumber LicenseNumber { get; private set; } = null!;
    public string Biography { get; private set; } = string.Empty;
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

    // Factory Method
    internal static Doctor Create(Guid userId, MedicalLicenseNumber licenseNumber, Guid medicalSpecialtyId, string biography, int consultationRoomNumber)
    {
        if (userId == Guid.Empty) throw new BusinessRuleValidationException("User ID cannot be empty.");
        if (medicalSpecialtyId == Guid.Empty) throw new BusinessRuleValidationException("Medical specialty ID cannot be empty.");
        if (consultationRoomNumber <= 0) throw new BusinessRuleValidationException("Consultation room number must be positive.");

        return new Doctor(userId, licenseNumber, medicalSpecialtyId, biography, consultationRoomNumber);
    }
}
