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
    public DateOnly DateOfBirth { get; init; }
    public BiologicalSex SexAtBirth { get; init; }
    public required Address HomeAddress { get; init; }
    public required EmailAddress Email { get; init; }
    public required PhoneNumber Phone { get; init; }
}
