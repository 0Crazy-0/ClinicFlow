using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Consent;

public sealed record MedicalRecordConsentGrantContext
{
    public required Patient TargetPatient { get; init; }
    public required FamilyMembership RequesterMembership { get; init; }
    public required MedicalRecord TargetMedicalRecord { get; init; }
    public required FamilyMembership RecipientMembership { get; init; }
    public bool HasEffectiveGrant { get; init; }
    public Guid RequesterUserId { get; init; }
    public DateTime ReferenceTime { get; init; }
}
