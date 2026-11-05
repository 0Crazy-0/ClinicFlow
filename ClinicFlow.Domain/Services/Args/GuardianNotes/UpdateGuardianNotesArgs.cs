namespace ClinicFlow.Domain.Services.Args.GuardianNotes;

public sealed record UpdateGuardianNotesArgs
{
    public required string Notes { get; init; }
}
