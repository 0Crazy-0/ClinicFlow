using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.RevokeFamilyMember;

public sealed record RevokeFamilyMemberCommand(Guid OwnerUserId, Guid PatientId) : IRequest;
