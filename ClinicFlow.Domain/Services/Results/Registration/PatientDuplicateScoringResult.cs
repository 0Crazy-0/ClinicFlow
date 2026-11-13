using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Contexts.Registration;

namespace ClinicFlow.Domain.Services.Results.Registration;

public sealed record PatientDuplicateScore(PatientDuplicateCandidate Candidate, int Score);

public sealed record PatientDuplicateScoringResult(IReadOnlyList<PatientDuplicateScore> Scores)
{
    /// <remarks>
    /// Applies to both the adult and minor duplicate scoring flows, whose maximal scores are 100
    /// and 90 respectively, so the same value represents a stricter relative similarity in the
    /// minor flow. Its meaning depends on the weight sum invariant documented in each scoring
    /// service.
    /// </remarks>
    public const int HighScoreThreshold = 70;

    public int MaxScore => Scores.Count == 0 ? 0 : Scores.Max(score => score.Score);
    public bool IsHighBand => MaxScore >= HighScoreThreshold;

    /// <summary>
    /// Refuses the registration flow when the strongest duplicate score reached the high band and no override reason justifies the creation.
    /// </summary>
    public void EnsureRegistrationAllowed(string? overrideReason)
    {
        if (IsHighBand && string.IsNullOrWhiteSpace(overrideReason))
            throw new DomainValidationException(
                DomainErrors.PatientRegistration.DuplicateSuspected
            );
    }
}
