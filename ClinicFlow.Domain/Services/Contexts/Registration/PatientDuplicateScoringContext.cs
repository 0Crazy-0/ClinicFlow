namespace ClinicFlow.Domain.Services.Contexts.Registration;

public sealed record class PatientDuplicateScoringContext
{
    public required IReadOnlyList<PatientDuplicateCandidate> Candidates { get; init; }
}
