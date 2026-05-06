using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;

public sealed record ReactivateDoctorProfileCommand(
    Guid DoctorId,
    string Biography,
    int ConsultationRoomNumber,
    string ConsultationRoomName,
    int ConsultationRoomFloor
) : IRequest;
