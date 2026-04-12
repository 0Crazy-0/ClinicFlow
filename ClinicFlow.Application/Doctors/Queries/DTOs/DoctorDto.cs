namespace ClinicFlow.Application.Doctors.Queries.DTOs;

public sealed record DoctorDto(
    Guid Id,
    Guid UserId,
    Guid MedicalSpecialtyId,
    string LicenseNumber,
    string Biography,
    int ConsultationRoomNumber
);
