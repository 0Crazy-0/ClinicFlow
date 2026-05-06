using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.SuspendDoctorProfile;

public sealed record SuspendDoctorProfileCommand(Guid DoctorId) : IRequest;
