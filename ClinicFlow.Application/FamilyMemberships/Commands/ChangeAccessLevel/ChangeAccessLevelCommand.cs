using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.ChangeAccessLevel;

public sealed record ChangeAccessLevelCommand(
    Guid RequesterUserId,
    Guid TargetUserId,
    Guid PatientId,
    FamilyMembershipAccessLevel NewAccessLevel
) : IRequest;
