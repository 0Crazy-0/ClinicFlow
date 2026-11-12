using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Contexts.Registration;

/// <summary>
/// Holds the world state of the guardian account and the family, recovered by the Application layer, at the moment of the minor link.
/// </summary>
public sealed record class MinorFamilyLinkContext
{
    public required Patient GuardianPatient { get; init; }

    /// <remarks>
    /// The Application layer is expected to resolve this membership already filtered to the
    /// <see cref="FamilyMembershipStatus.Active"/> status. The link service still validates
    /// the status as a defensive invariant, so a membership in any other state is rejected.
    /// </remarks>
    public required FamilyMembership GuardianSelfMembership { get; init; }
    public int ActiveFamilyMemberCount { get; init; }
    public DateTime ReferenceTime { get; init; }
}
