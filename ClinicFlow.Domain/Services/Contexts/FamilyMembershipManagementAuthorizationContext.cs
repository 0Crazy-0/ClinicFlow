using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record FamilyMembershipManagementAuthorizationContext
{
    public required Patient Patient { get; init; }
    public DateTime ReferenceTime { get; init; }
    public Guid RequesterUserId { get; init; }
    public Guid TargetUserId { get; init; }
    public bool RequesterIsPatientsSelf { get; init; }
}
