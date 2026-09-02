using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record AccessLevelChangeAuthorizationContext
{
    public required Patient Patient { get; init; }
    public DateTime ReferenceTime { get; init; }
    public bool RequesterHasSelfMembership { get; init; }
    public bool RequesterIsPatientsSelf { get; init; }
    public bool RequesterHasActiveMembershipWithPatient { get; init; }
}
