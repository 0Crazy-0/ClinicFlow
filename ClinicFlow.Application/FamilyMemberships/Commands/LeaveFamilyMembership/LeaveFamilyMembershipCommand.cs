using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.LeaveFamilyMembership;

public sealed record LeaveFamilyMembershipCommand(Guid UserId, Guid PatientId) : IRequest;
