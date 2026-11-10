using ClinicFlow.Domain.Services.Contexts.Registration;

namespace ClinicFlow.Domain.Services.Results.Registration;

public sealed record PatientDuplicateScore(PatientDuplicateCandidate Candidate, int Score);

public sealed record PatientDuplicateScoringResult(IReadOnlyList<PatientDuplicateScore> Scores)
{
    public int MaxScore => Scores.Count == 0 ? 0 : Scores.Max(score => score.Score);
    public bool IsHighBand => MaxScore >= PatientDuplicateScoringService.HighScoreThreshold;
}
