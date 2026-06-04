using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.AddFamilyMember;

public sealed record AddFamilyMemberCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    PatientRelationship Relationship
) : IRequest<Guid>;
