using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public sealed record GuardianReschedulingArgs
{
    public required Patient TargetPatient { get; init; }
    public required FamilyMembership InitiatorMembership { get; init; }
    public Guid InitiatorUserId { get; init; }
    public DateOnly NewDate { get; init; }
    public required TimeRange NewTimeRange { get; init; }
    public bool IsInitiatorPhoneVerified { get; init; }
    public string? NewGuardianNotes { get; init; }
}
