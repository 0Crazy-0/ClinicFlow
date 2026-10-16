namespace ClinicFlow.Domain.Services.Args.GuardianInvolvement;

public sealed record DoctorGuardianInvolvementArgs
{
    public bool GuardianInvolvementDeemedAppropriate { get; init; }
}
