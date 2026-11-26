namespace ClinicFlow.Application.MedicalRecordConsentGrants.Queries.DTOs;

public sealed record MedicalRecordConsentGrantDto(
    Guid Id,
    Guid PatientId,
    Guid RecipientMembershipId,
    Guid MedicalRecordId,
    Guid SignedByUserId,
    DateTimeOffset SignedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    bool IsEffective
);
