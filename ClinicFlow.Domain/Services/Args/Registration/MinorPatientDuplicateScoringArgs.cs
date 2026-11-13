using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

/// <summary>
/// Holds the registration form data of the dependent minor being registered, used to score against existing candidates for duplicate detection. The dependent minor form carries no contact data, so email and phone do not participate in the score.
/// </summary>
public sealed record MinorPatientDuplicateScoringArgs
{
    public required string FirstName { get; init; }
    public required string FirstSurname { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public BiologicalSex SexAtBirth { get; init; }
    public required Address HomeAddress { get; init; }

    /// <summary>
    /// Holds whether the registration form supplied the SSN last four digits, so the match weight applies only when the candidate verified against them.
    /// </summary>
    public bool HasSsnLast4 { get; init; }
}
