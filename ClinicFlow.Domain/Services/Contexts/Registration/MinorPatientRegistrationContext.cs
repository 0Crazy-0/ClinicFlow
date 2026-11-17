using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Registration;

/// <summary>
/// Holds the world state at the moment of the dependent minor registration: the guardian account
///  state, the duplicate candidates recovered from persistence, and the identity of the receptionist performing the registration.
/// </summary>
public sealed record class MinorPatientRegistrationContext
{
    public required IReadOnlyList<PatientDuplicateCandidate> DuplicateCandidates { get; init; }
    public required Patient GuardianPatient { get; init; }

    /// <remarks>
    /// The Application layer is expected to resolve this membership already filtered to the
    /// Active status. The delegated link validation re-checks it as a defensive invariant.
    /// </remarks>
    public required FamilyMembership GuardianSelfMembership { get; init; }
    public int ActiveFamilyMemberCount { get; init; }
    public Guid ReceptionistUserId { get; init; }
    public DateTime ReferenceTime { get; init; }
}
