using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.GuardianInvolvement;

public sealed record class DoctorGuardianInvolvementContext
{
    public required Doctor InitiatorDoctor { get; init; }
}
