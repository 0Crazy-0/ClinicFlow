namespace ClinicFlow.Domain.Services.Args.GuardianInvolvement;

public sealed record DoctorGuardianInvolvementArgs
{
    public Guid InitiatorDoctorId { get; init; }
    public bool GuardianInvolvementDeemedAppropriate { get; init; }
}
