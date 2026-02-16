using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.Entities;

public class Doctor : BaseEntity
{
    public Guid UserId { get; init; }
    public Guid MedicalSpecialtyId { get; init; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public string Biography { get; private set; } = string.Empty;
    public int ConsultationRoomNumber { get; private set; }

    // EF Core constructor
    private Doctor() { }

    private Doctor(Guid userId, string licenseNumber, Guid medicalSpecialtyId, string biography, int consultationRoomNumber)
    {
        UserId = userId;
        LicenseNumber = licenseNumber;
        MedicalSpecialtyId = medicalSpecialtyId;
        Biography = biography;
        ConsultationRoomNumber = consultationRoomNumber;
    }

    // Factory Method
    internal static Doctor Create(Guid userId, string licenseNumber, Guid medicalSpecialtyId, string biography, int consultationRoomNumber)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber)) throw new BusinessRuleValidationException("License number cannot be empty.");
        if (consultationRoomNumber <= 0) throw new BusinessRuleValidationException("Consultation room number must be positive.");

        return new Doctor(userId, licenseNumber, medicalSpecialtyId, biography, consultationRoomNumber);
    }
}
