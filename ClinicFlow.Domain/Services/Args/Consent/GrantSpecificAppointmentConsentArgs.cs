namespace ClinicFlow.Domain.Services.Args.Consent;

public sealed record GrantSpecificAppointmentConsentArgs
{
    public Guid RequesterUserId { get; init; }
    public DateTime ReferenceTime { get; init; }
}
