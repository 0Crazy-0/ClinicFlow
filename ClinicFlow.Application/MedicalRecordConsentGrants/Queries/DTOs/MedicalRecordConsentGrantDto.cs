namespace ClinicFlow.Application.MedicalRecordConsentGrants.Queries.DTOs;

public sealed record MedicalRecordConsentGrantDto(
    Guid Id,
    Guid PatientId,
    Guid RecipientMembershipId,
    Guid MedicalRecordId,
    Guid SignedByUserId,
    DateTime SignedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    bool IsEffective
);
