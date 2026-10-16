using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.FamilyMembershipManagement;

public sealed record FamilyMembershipManagementAuthorizationContext
{
    public required Patient TargetPatient { get; init; }
    public DateTime ReferenceTime { get; init; }
    public Guid RequesterUserId { get; init; }
    public Guid TargetUserId { get; init; }
    public bool RequesterIsPatientsSelf { get; init; }
}
