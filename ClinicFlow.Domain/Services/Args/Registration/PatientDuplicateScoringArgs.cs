using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

/// <summary>
/// Holds the registration form data of the patient being registered, used to score against existing candidates for duplicate detection.
/// </summary>
public sealed record PatientDuplicateScoringArgs
{
    public required string FirstName { get; init; }
    public required string FirstSurname { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required BiologicalSex SexAtBirth { get; init; }
    public required Address HomeAddress { get; init; }

    /// <summary>
    /// Holds whether the registration form supplied the SSN last four digits, so the match weight applies only when the candidate verified against them.
    /// </summary>
    public bool HasSsnLast4 { get; init; }
    public EmailAddress? Email { get; init; }
    public PhoneNumber? Phone { get; init; }
}
