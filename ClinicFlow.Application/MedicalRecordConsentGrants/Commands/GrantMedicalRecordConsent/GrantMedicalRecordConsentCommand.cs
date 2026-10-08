using MediatR;

namespace ClinicFlow.Application.MedicalRecordConsentGrants.Commands.GrantMedicalRecordConsent;

public sealed record GrantMedicalRecordConsentCommand(
    Guid RequesterUserId,
    Guid PatientId,
    Guid MedicalRecordId,
    Guid RecipientMembershipId
) : IRequest<Guid>;
