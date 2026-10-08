using MediatR;

namespace ClinicFlow.Application.MedicalRecordConsentGrants.Commands.RevokeMedicalRecordConsentGrant;

public sealed record RevokeMedicalRecordConsentGrantCommand(
    Guid RequesterUserId,
    Guid ConsentGrantId
) : IRequest;
