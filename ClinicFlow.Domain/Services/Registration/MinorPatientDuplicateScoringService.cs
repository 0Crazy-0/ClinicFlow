using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.Services.Registration.Shared;
using ClinicFlow.Domain.Services.Results.Registration;

namespace ClinicFlow.Domain.Services.Registration;

/// <remarks>
/// Score weights sum to exactly 90 because the dependent minor registration form carries no
/// contact data, so email and phone do not participate in the score. Shared dimension weights
/// mirror <see cref="PatientDuplicateScoringService"/>, and the threshold on
/// <see cref="PatientDuplicateScoringResult.HighScoreThreshold"/> assumes the weight sum
/// invariant; changing any weight requires rebalancing the others.
/// </remarks>
public static class MinorPatientDuplicateScoringService
{
    public const int DateOfBirthWeight = 30;
    public const int SurnameSimilarityWeight = 20;
    public const int GivenNameSimilarityWeight = 10;
    public const int SsnLast4MatchWeight = 20;
    public const int SexMatchWeight = 5;

    public static PatientDuplicateScoringResult Score(
        MinorPatientDuplicateScoringArgs args,
        PatientDuplicateScoringContext context
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.HomeAddress);
        ArgumentNullException.ThrowIfNull(context);

        var scores = context
            .Candidates.Select(candidate => new PatientDuplicateScore(
                candidate,
                ComputeScore(args, candidate)
            ))
            .ToArray();

        return new PatientDuplicateScoringResult(scores);
    }

    private static int ComputeScore(
        MinorPatientDuplicateScoringArgs form,
        PatientDuplicateCandidate candidate
    )
    {
        var score = 0.0;

        if (form.DateOfBirth == candidate.DateOfBirth)
            score += DateOfBirthWeight;

        score +=
            SurnameSimilarityWeight
            * PatientDuplicateScoringMath.Similarity(
                StringNormalizer.Canonicalize(form.FirstSurname),
                StringNormalizer.Canonicalize(candidate.FullName.FirstSurname)
            );

        score +=
            GivenNameSimilarityWeight
            * PatientDuplicateScoringMath.Similarity(
                StringNormalizer.Canonicalize(form.FirstName),
                StringNormalizer.Canonicalize(candidate.FullName.FirstName)
            );

        if (form.HasSsnLast4 && candidate.SsnLast4Matches)
            score += SsnLast4MatchWeight;

        if (form.SexAtBirth == candidate.SexAtBirth)
            score += SexMatchWeight;

        score += PatientDuplicateScoringMath.ComputeAddressWeight(
            form.HomeAddress,
            candidate.HomeAddress
        );

        return (int)Math.Round(score, MidpointRounding.AwayFromZero);
    }
}
