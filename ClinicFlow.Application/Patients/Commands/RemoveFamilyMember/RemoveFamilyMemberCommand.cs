using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.RemoveFamilyMember;

public sealed record RemoveFamilyMemberCommand(
    Guid PatientId,
    Guid InitiatorUserId,
    PatientRelationship InitiatorRelationship
) : IRequest;
