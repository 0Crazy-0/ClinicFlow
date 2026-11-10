using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Contexts.Registration;

public sealed record PatientDuplicateCandidate
{
    public required Guid PatientId { get; init; }
    public required PersonName FullName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required BiologicalSex SexAtBirth { get; init; }
    public required Address HomeAddress { get; init; }

    /// <summary>
    /// Holds the candidate's account email resolved through its self membership, or null when the patient has
    /// no account, such as a minor patient registered without a dedicated user account.
    /// </summary>
    public EmailAddress? Email { get; init; }

    /// <summary>
    /// Holds the candidate's account phone resolved through its self membership, or null when the patient has
    /// no account, such as a minor patient registered without a dedicated user account.
    /// </summary>
    public PhoneNumber? Phone { get; init; }

    public string? SsnLast4Hash { get; init; }

    /// <summary>
    /// Holds the verification result the Application layer computes by hashing the form's SSN last four digits against this candidate's stored hash.
    /// </summary>
    public bool SsnLast4Matches { get; init; }
}
