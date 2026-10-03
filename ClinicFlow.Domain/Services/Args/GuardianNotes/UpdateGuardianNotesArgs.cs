using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Args.GuardianNotes;

public sealed record UpdateGuardianNotesArgs
{
    public required Patient TargetPatient { get; init; }
    public required FamilyMembership InitiatorMembership { get; init; }
    public Guid InitiatorUserId { get; init; }
    public string? Notes { get; init; }
}
