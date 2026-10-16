namespace ClinicFlow.Domain.Services.Args.GuardianNotes;

public sealed record UpdateGuardianNotesArgs
{
    public string? Notes { get; init; }
}
