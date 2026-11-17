using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Registration;

/// <summary>
/// Holds the world state at the moment of the registration: the pending account the handler
/// created, the duplicate candidates recovered from persistence, and the identity of the
/// receptionist performing the registration.
/// </summary>
public sealed record class PatientRegistrationContext
{
    public required IReadOnlyList<PatientDuplicateCandidate> DuplicateCandidates { get; init; }

    /// <remarks>
    /// The account is created by the Application handler before the registration, so the
    /// service only wires the self membership to it and leaves it untouched.
    /// </remarks>
    public required User PendingUser { get; init; }
    public Guid ReceptionistUserId { get; init; }
    public DateTime ReferenceTime { get; init; }
}
