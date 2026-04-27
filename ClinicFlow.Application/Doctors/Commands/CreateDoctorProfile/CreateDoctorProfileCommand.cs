using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;

public sealed record CreateDoctorProfileCommand(
    Guid UserId,
    string LicenseNumber,
    Guid MedicalSpecialtyId,
    string Biography,
    int ConsultationRoomNumber,
    string ConsultationRoomName,
    int ConsultationRoomFloor
) : IRequest<Guid>;
