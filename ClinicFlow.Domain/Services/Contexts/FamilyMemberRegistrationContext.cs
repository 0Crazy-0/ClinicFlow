using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record class FamilyMemberRegistrationContext
{
    public Patient? ExistingPatient { get; init; }
    public bool HasExistingMembershipWithOwner { get; init; }
    public int ActiveFamilyMemberCount { get; init; }
    public int OwnerAgeInYears { get; init; }
    public Guid OwnerUserId { get; init; }
    public DateTime ReferenceTime { get; init; }
}
