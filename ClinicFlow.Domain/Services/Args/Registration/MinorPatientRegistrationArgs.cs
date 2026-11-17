using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

/// <summary>
/// Holds the reception form data of the dependent minor patient being registered,
/// used both to create the profile and to score against existing candidates for duplicate detection.
/// </summary>
public sealed record MinorPatientRegistrationArgs
{
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string FirstSurname { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public BiologicalSex SexAtBirth { get; init; }
    public required Address HomeAddress { get; init; }

    /// <summary>
    /// Holds whether the registration form supplied the SSN last four digits, so the
    /// candidate match weight applies only when the Application layer verified against them.
    /// </summary>
    public bool HasSsnLast4 { get; init; }
    public string? DuplicateOverrideReason { get; init; }
    public required PatientRelationship Relationship { get; init; }
    public required LegalAuthorityType LegalAuthority { get; init; }
}
