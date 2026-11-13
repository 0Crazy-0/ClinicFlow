using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.Services.Registration.Shared;
using ClinicFlow.Domain.Services.Results.Registration;

namespace ClinicFlow.Domain.Services.Registration;

/// <remarks>
/// Score weights sum to exactly 100 for this contact-bearing shape, so a candidate score is
/// interpretable as a percentage of maximal similarity. The minor counterpart,
/// <see cref="MinorPatientDuplicateScoringService"/>, sums to 90 because contact dimensions do
/// not apply to it. The threshold on <see cref="PatientDuplicateScoringResult.HighScoreThreshold"/>
/// assumes the weight sum invariant; changing any weight requires rebalancing the others.
/// </remarks>
public static class PatientDuplicateScoringService
{
    public const int DateOfBirthWeight = 30;
    public const int SurnameSimilarityWeight = 20;
    public const int GivenNameSimilarityWeight = 10;
    public const int SsnLast4MatchWeight = 20;
    public const int SexMatchWeight = 5;
    public const int EmailMatchWeight = 5;
    public const int PhoneMatchWeight = 5;

    public static PatientDuplicateScoringResult Score(
        PatientDuplicateScoringArgs args,
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
        PatientDuplicateScoringArgs form,
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

        // the candidate may lack contact data: a patient who has not created an
        // account of their own yet, for example a minor linked to an existing adult
        // patient, has no email or phone of their own. The form side always supplies both.
        if (form.Email == candidate.Email)
            score += EmailMatchWeight;

        if (candidate.Phone?.Digits == form.Phone.Digits)
            score += PhoneMatchWeight;

        score += PatientDuplicateScoringMath.ComputeAddressWeight(
            form.HomeAddress,
            candidate.HomeAddress
        );

        return (int)Math.Round(score, MidpointRounding.AwayFromZero);
    }
}
