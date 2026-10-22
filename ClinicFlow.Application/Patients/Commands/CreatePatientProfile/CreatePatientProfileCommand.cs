using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CreatePatientProfile;

public sealed record CreatePatientProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    BiologicalSex SexAtBirth,
    string AddressLine,
    AddressState AddressState,
    string AddressPostalCode,
    Country AddressCountry
) : IRequest<Guid>;
