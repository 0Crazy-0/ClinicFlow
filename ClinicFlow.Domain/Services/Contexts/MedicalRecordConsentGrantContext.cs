using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record MedicalRecordConsentGrantContext
{
    public required Patient Patient { get; init; }
    public required FamilyMembership RequesterMembership { get; init; }
    public required MedicalRecord MedicalRecord { get; init; }
    public required FamilyMembership RecipientMembership { get; init; }
    public bool HasEffectiveGrant { get; init; }
}
