using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Entities;

public class Doctor : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid MedicalSpecialtyId { get; private set; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public string Biography { get; private set; } = string.Empty;
    public int ConsultationRoomNumber { get; private set; }
    public Guid Specialty { get; private set; }
    
    // EF Core constructor
    private Doctor() { }
    public Doctor(Guid userId, string licenseNumber, Guid medicalSpecialtyId, string biography, int consultationRoomNumber)
    {
        UserId = userId;
        LicenseNumber = licenseNumber;
        MedicalSpecialtyId = medicalSpecialtyId;
        Biography = biography;
        ConsultationRoomNumber = consultationRoomNumber;
    }
}
