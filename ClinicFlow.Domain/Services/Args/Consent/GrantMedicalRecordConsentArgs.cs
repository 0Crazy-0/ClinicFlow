namespace ClinicFlow.Domain.Services.Args.Consent;

public sealed record GrantMedicalRecordConsentArgs
{
    public Guid RequesterUserId { get; init; }
    public DateTime ReferenceTime { get; init; }
}
