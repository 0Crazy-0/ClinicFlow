using MediatR;

namespace ClinicFlow.Application.Patients.Commands.RemoveFamilyMember;

public sealed record RemoveFamilyMemberCommand(Guid PatientId, Guid UserId) : IRequest;
