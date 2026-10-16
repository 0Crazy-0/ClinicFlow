using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record class UpdateGuardianNotesContext
{
    public required Patient TargetPatient { get; init; }
    public required FamilyMembership InitiatorMembership { get; init; }
}
