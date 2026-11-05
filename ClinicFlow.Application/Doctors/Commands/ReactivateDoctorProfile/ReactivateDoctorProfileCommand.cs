using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;

public sealed record ReactivateDoctorProfileCommand(Guid DoctorId) : IRequest;
