using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CreatePatientProfile;

public sealed record CreatePatientProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth
) : IRequest<Guid>;
