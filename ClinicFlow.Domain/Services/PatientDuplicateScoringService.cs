using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Common.ThirdParty;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.Services.Results.Registration;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <remarks>
/// Score weights sum to exactly 100, so a candidate score is interpretable as a
/// percentage of maximal similarity. Name and address line similarity are computed
/// with the Levenshtein edit distance against canonicalized values, and
/// <see cref="HighScoreThreshold"/> assumes the weight sum invariant; changing any weight
/// requires rebalancing the others.
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
    public const int AddressWeight = 5;
    public const int HighScoreThreshold = 70;

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

    /// <summary>
    /// Refuses the registration flow when the strongest duplicate score reached the high band and no override reason justifies the creation.
    /// </summary>
    public static void EnsureRegistrationAllowed(
        PatientDuplicateScoringResult result,
        string? overrideReason
    )
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsHighBand && string.IsNullOrWhiteSpace(overrideReason))
            throw new DomainValidationException(
                DomainErrors.PatientRegistration.DuplicateSuspected
            );
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
            * Similarity(
                StringNormalizer.Canonicalize(form.FirstSurname),
                StringNormalizer.Canonicalize(candidate.FullName.FirstSurname)
            );

        score +=
            GivenNameSimilarityWeight
            * Similarity(
                StringNormalizer.Canonicalize(form.FirstName),
                StringNormalizer.Canonicalize(candidate.FullName.FirstName)
            );

        if (form.HasSsnLast4 && candidate.SsnLast4Matches)
            score += SsnLast4MatchWeight;

        if (form.SexAtBirth == candidate.SexAtBirth)
            score += SexMatchWeight;

        if (MatchesEmail(form.Email, candidate.Email))
            score += EmailMatchWeight;

        if (MatchesPhone(form.Phone, candidate.Phone))
            score += PhoneMatchWeight;

        score += ComputeAddressWeight(form.HomeAddress, candidate.HomeAddress);

        return (int)Math.Round(score, MidpointRounding.AwayFromZero);
    }

    private static double ComputeAddressWeight(Address formAddress, Address candidateAddress)
    {
        if (
            formAddress.State != candidateAddress.State
            || formAddress.PostalCode != candidateAddress.PostalCode
        )
            return 0;

        return AddressWeight
            * Similarity(
                StringNormalizer.Canonicalize(formAddress.Line),
                StringNormalizer.Canonicalize(candidateAddress.Line)
            );
    }

    private static bool MatchesEmail(EmailAddress? formEmail, EmailAddress? candidateEmail)
    {
        // Stryker disable once Logical: see docs/tooling/stryker.md, section "9. PatientDuplicateScoringService Email Null Guard Equivalence"
        if (formEmail is null || candidateEmail is null)
            return false;

        return formEmail == candidateEmail;
    }

    private static bool MatchesPhone(PhoneNumber? formPhone, PhoneNumber? candidatePhone)
    {
        if (formPhone is null || candidatePhone is null)
            return false;

        return formPhone.Digits == candidatePhone.Digits;
    }

    private static double Similarity(string left, string right)
    {
        if (left.Length == 0 && right.Length == 0)
            return 1.0;

        var distance = Levenshtein.Distance(left, right);

        return 1.0 - distance / (double)Math.Max(left.Length, right.Length);
    }
}
