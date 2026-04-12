using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.UpdateDoctorProfile;

public sealed record UpdateDoctorProfileCommand(
    Guid DoctorId,
    string Biography,
    int ConsultationRoomNumber
) : IRequest;
