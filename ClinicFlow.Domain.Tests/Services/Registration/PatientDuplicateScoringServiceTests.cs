using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.Services.Results.Registration;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Services.Registration;

public class PatientDuplicateScoringServiceTests
{
    [Fact]
    public void Score_ShouldReturnEmptyScores_WhenCandidateListIsEmpty()
    {
        // Arrange
        var form = CreateStrangerForm();
        var context = new PatientDuplicateScoringContext { Candidates = [] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().BeEmpty();
        result.MaxScore.Should().Be(0);
        result.IsHighBand.Should().BeFalse();
    }

    [Fact]
    public void Score_ShouldReturnZeroScore_WhenCandidateIsACompleteStranger()
    {
        // Arrange
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate();

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlyDateOfBirthMatches()
    {
        // Arrange
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with { DateOfBirth = form.DateOfBirth };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(30);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlySurnameIsIdentical()
    {
        // Arrange
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("Juan", null, form.FirstSurname),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(20);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlyGivenNameIsIdentical()
    {
        // Arrange
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create(form.FirstName, null, "John"),
        };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(10);
    }

    [Fact]
    public void Score_ShouldScoreSurnameSimilarity_WhenSurnamesHaveEditDistanceTwo()
    {
        // Arrange
        // canonicalized "garcia" and "garza" have edit distance 2 of a maximum 6,
        // so the surname contribution is 20 * (1 - 2/6) = 13.333... rounded away from zero to 13.
        var form = CreateStrangerForm() with
        {
            FirstSurname = "Garcia",
        };

        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("Juan", null, "Garza"),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(13);
    }

    [Fact]
    public void Score_ShouldScoreGivenNameSimilarity_WhenOnlyLastCharacterDiffers()
    {
        // Arrange
        // canonicalized "remy" and "remi" have edit distance 1 of a maximum 4,
        // so the given name contribution is 10 * 0.75 = 7.5 rounded away from zero to 8.
        var form = CreateStrangerForm() with
        {
            FirstName = "Remy",
        };

        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("Remi", null, "John"),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(8);
    }

    [Fact]
    public void Score_ShouldNotScoreSurnameSimilarity_WhenFormSurnameNormalizesToEmpty()
    {
        // Arrange
        // a first surname composed only of non-spacing marks canonicalizes
        // to an empty string, so it is not similar to any non-empty surname.
        var form = CreateStrangerForm() with
        {
            FirstSurname = "\u0301",
        };
        var candidate = CreateStrangerCandidate();
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldNotScoreSurnameSimilarity_WhenCandidateSurnameNormalizesToEmpty()
    {
        // Arrange
        // a candidate full name composed only of non-spacing marks canonicalizes
        // to an empty string, so it is not similar to any non-empty surname.
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("\u0301\u0301", null, "\u0301\u0301"),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldScoreSurnameSimilarity_WhenBothSurnamesNormalizeToEmpty()
    {
        // Arrange
        // surnames composed only of non-spacing marks canonicalize to empty
        // strings, and two empty canonicalized values are maximally similar,
        // so the full surname weight applies. The given name cross comparison
        // of "remy" against "juan" contributes zero.
        var form = CreateStrangerForm() with
        {
            FirstSurname = "\u0301\u0301",
        };

        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("Juan", null, "\u0301\u0301"),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(20);
    }

    [Fact]
    public void Score_ShouldScoreGivenNameSimilarity_WhenBothGivenNamesNormalizeToEmpty()
    {
        // Arrange
        // given names composed only of non-spacing marks canonicalize to empty
        // strings, and two empty canonicalized values are maximally similar,
        // so the full given name weight applies. The surname cross comparison
        // of "levy" against "john" contributes zero.
        var form = CreateStrangerForm() with
        {
            FirstName = "\u0301\u0301",
        };

        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("\u0301\u0301", null, "John"),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(10);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenFormSuppliesSsnLast4AndCandidateMatches()
    {
        // Arrange
        var form = CreateStrangerForm() with
        {
            HasSsnLast4 = true,
        };

        var candidate = CreateStrangerCandidate() with { SsnLast4Matches = true };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(20);
    }

    [Fact]
    public void Score_ShouldNotScoreSsnLast4_WhenFormDoesNotSupplySsnLast4()
    {
        // Arrange
        // the form supplies no SSN last 4, yet the candidate reports a match.
        // This state should never happen in production, because the match is only
        // computed when the form supplies the value. It is built on purpose to verify
        // that the scoring service guards on form.HasSsnLast4 by itself, without
        // trusting the context to be well formed.
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with { SsnLast4Matches = true };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldNotScoreSsnLast4_WhenCandidateDoesNotMatch()
    {
        // Arrange
        var form = CreateStrangerForm() with
        {
            HasSsnLast4 = true,
        };
        var candidate = CreateStrangerCandidate();
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlySexAtBirthMatches()
    {
        // Arrange
        var form = CreateStrangerForm() with
        {
            SexAtBirth = BiologicalSex.Female,
        };

        var candidate = CreateStrangerCandidate(); // biologic sex is famale
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(5);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlyEmailMatches()
    {
        // Arrange
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with { Email = EmailAddress.Create(form.Email!) };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(5);
    }

    [Fact]
    public void Score_ShouldNotScoreEmail_WhenBothEmailsAreMissing()
    {
        // Arrange
        // neither the form nor the candidate has an email. This is a valid
        // production state: a minor linked to an existing adult patient may not have
        // created an account of their own yet, and the candidate can be another such
        // minor.
        var form = CreateStrangerForm() with
        {
            Email = null,
        };
        var candidate = CreateStrangerCandidate() with { Email = null };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlyPhoneMatches()
    {
        // Arrange
        // digits are compared, so formatting differences still match.
        var form = CreateStrangerForm(); // phon numer is: 555-1234
        var candidate = CreateStrangerCandidate() with { Phone = PhoneNumber.Create("(555) 1234") };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(5);
    }

    [Fact]
    public void Score_ShouldNotScorePhone_WhenBothPhonesAreMissing()
    {
        // Arrange
        // neither the form nor the candidate has a phone. This is a valid
        // production state: the phone belongs to the user account, not to the patient,
        // so a patient who has not created an account of their own yet (for example,
        // a minor linked to an existing adult patient) has no phone.
        var form = CreateStrangerForm() with
        {
            Phone = null,
        };
        var candidate = CreateStrangerCandidate() with { Phone = null };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldNotScorePhone_WhenOnlyTheFormHasPhone()
    {
        // Arrange
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with { Phone = null }; // Only for minor accounts without their own account
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenHomeAddressIsIdentical()
    {
        // Arrange
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with { HomeAddress = form.HomeAddress };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(5);
    }

    [Fact]
    public void Score_ShouldNotScoreAddress_WhenLineIsIdenticalButStateDiffers()
    {
        // Arrange
        var form = CreateStrangerForm(); // line is: Maple Street
        var candidate = CreateStrangerCandidate() with
        {
            HomeAddress = Address.Create(
                "Maple Street",
                AddressState.Florida,
                "33101",
                Country.UnitedStates
            ),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldNotScoreAddress_WhenLineIsIdenticalButPostalCodeDiffers()
    {
        // Arrange
        var form = CreateStrangerForm(); // postal code is: 75001
        var candidate = CreateStrangerCandidate() with
        {
            HomeAddress = Address.Create(
                "Maple Street",
                AddressState.Texas,
                "75002",
                Country.UnitedStates
            ),
        };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldScoreAddressSimilarity_WhenLinesShareSixCharacters()
    {
        // Arrange
        // canonicalized "calle uno" and "calle dos" have edit distance 3 of a maximum 9,
        // so the address contribution is 5 * (1 - 3/9) = 3.333... rounded away from zero to 3.
        var form = CreateStrangerForm() with
        {
            HomeAddress = Address.Create(
                "Calle Uno",
                AddressState.Texas,
                "75001",
                Country.UnitedStates
            ),
        };

        var candidate = CreateStrangerCandidate() with
        {
            HomeAddress = Address.Create(
                "Calle Dos",
                AddressState.Texas,
                "75001",
                Country.UnitedStates
            ),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(3);
    }

    [Fact]
    public void Score_ShouldScoreAddressSimilarity_WhenLinesDifferInLength()
    {
        // Arrange
        // canonicalized "calle uno" and "calle unos" have edit distance 1 of a maximum 10,
        // so the address contribution is 5 * 0.9 = 4.5 rounded away from zero to 5.
        var form = CreateStrangerForm() with
        {
            HomeAddress = Address.Create(
                "Calle Uno",
                AddressState.Texas,
                "75001",
                Country.UnitedStates
            ),
        };
        var candidate = CreateStrangerCandidate() with
        {
            HomeAddress = Address.Create(
                "Calle Unos",
                AddressState.Texas,
                "75001",
                Country.UnitedStates
            ),
        };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(5);
    }

    [Fact]
    public void Score_ShouldScoreEveryCandidate_WhenMultipleCandidatesExist()
    {
        // Arrange
        var form = CreateStrangerForm();
        var firstCandidate = CreateStrangerCandidate();
        var secondCandidate = CreateStrangerCandidate() with { DateOfBirth = form.DateOfBirth };

        var context = new PatientDuplicateScoringContext
        {
            Candidates = [firstCandidate, secondCandidate],
        };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result
            .Scores.Should()
            .ContainSingle(score => score.Candidate == firstCandidate)
            .Which.Score.Should()
            .Be(0);

        result
            .Scores.Should()
            .ContainSingle(score => score.Candidate == secondCandidate)
            .Which.Score.Should()
            .Be(30);

        result.MaxScore.Should().Be(30);
    }

    [Fact]
    public void Score_ShouldReturnHighestBandScore_WhenFormMatchesCandidateCompletely()
    {
        // Arrange:
        // 30 + 20 + 10 + 20 + 5 + 5 + 5 + 5 = 100.
        var form = CreateStrangerForm() with
        {
            HasSsnLast4 = true,
        };

        var candidate = CreateMatchingCandidate();
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = PatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(100);
        result.MaxScore.Should().Be(100);
        result.IsHighBand.Should().BeTrue();
    }

    [Fact]
    public void Score_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () => PatientDuplicateScoringService.Score(null!, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Score_ShouldThrowArgumentNullException_WhenHomeAddressIsNull()
    {
        // Arrange
        var form = CreateStrangerForm() with
        {
            HomeAddress = null!,
        };

        // Act
        var act = () => PatientDuplicateScoringService.Score(form, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Score_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () => PatientDuplicateScoringService.Score(CreateStrangerForm(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureRegistrationAllowed_ShouldNotThrow_WhenStrongestScoreReachesHighBandButOverrideIsProvided()
    {
        // Arrange
        var result = CreateScoringResult(PatientDuplicateScoringService.HighScoreThreshold);

        // Act
        var act = () =>
            PatientDuplicateScoringService.EnsureRegistrationAllowed(result, "crazy reason");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureRegistrationAllowed_ShouldNotThrow_WhenStrongestScoreIsBelowHighBand()
    {
        // Arrange
        var result = CreateScoringResult(PatientDuplicateScoringService.HighScoreThreshold - 1);

        // Act
        var act = () => PatientDuplicateScoringService.EnsureRegistrationAllowed(result, null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureRegistrationAllowed_ShouldThrowArgumentNullException_WhenResultIsNull()
    {
        // Arrange & Act
        var act = () => PatientDuplicateScoringService.EnsureRegistrationAllowed(null!, null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureRegistrationAllowed_ShouldThrowDomainValidationException_WhenStrongestScoreReachesHighBand()
    {
        // Arrange
        var result = CreateScoringResult(PatientDuplicateScoringService.HighScoreThreshold);

        // Act
        var act = () => PatientDuplicateScoringService.EnsureRegistrationAllowed(result, null);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.PatientRegistration.DuplicateSuspected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureRegistrationAllowed_ShouldThrowDomainValidationException_WhenOverrideReasonIsWhitespaceOnly(
        string overrideReason
    )
    {
        // Arrange
        var result = CreateScoringResult(PatientDuplicateScoringService.HighScoreThreshold);

        // Act
        var act = () =>
            PatientDuplicateScoringService.EnsureRegistrationAllowed(result, overrideReason);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.PatientRegistration.DuplicateSuspected);
    }

    private static PatientDuplicateScoringContext CreateValidContext() =>
        new() { Candidates = [CreateStrangerCandidate()] };

    private static PatientDuplicateScoringResult CreateScoringResult(int score)
    {
        var candidate = CreateStrangerCandidate();

        return new PatientDuplicateScoringResult([new PatientDuplicateScore(candidate, score)]);
    }

    private static PatientDuplicateScoringArgs CreateStrangerForm() =>
        new()
        {
            FirstName = "Remy",
            FirstSurname = "Levy",
            DateOfBirth = new DateOnly(2000, 1, 1),
            SexAtBirth = BiologicalSex.Male,
            HomeAddress = Address.Create(
                "Maple Street",
                AddressState.Texas,
                "75001",
                Country.UnitedStates
            ),
            Email = EmailAddress.Create("remy@clinic.com"),
            Phone = PhoneNumber.Create("555-1234"),
        };

    private static PatientDuplicateCandidate CreateStrangerCandidate() =>
        new()
        {
            PatientId = Guid.CreateVersion7(),
            FullName = PersonName.Create("Juan", null, "John"),
            DateOfBirth = new DateOnly(1999, 9, 9),
            SexAtBirth = BiologicalSex.Female,
            HomeAddress = Address.Create(
                "Oak Street",
                AddressState.Florida,
                "33101",
                Country.UnitedStates
            ),
            Email = EmailAddress.Create("juan@clinic.com"),
            Phone = PhoneNumber.Create("9876543"),
        };

    private static PatientDuplicateCandidate CreateMatchingCandidate() =>
        new()
        {
            PatientId = Guid.CreateVersion7(),
            FullName = PersonName.Create("Remy", null, "Levy"),
            DateOfBirth = new DateOnly(2000, 1, 1),
            SexAtBirth = BiologicalSex.Male,
            HomeAddress = Address.Create(
                "Maple Street",
                AddressState.Texas,
                "75001",
                Country.UnitedStates
            ),
            Email = EmailAddress.Create("remy@clinic.com"),
            Phone = PhoneNumber.Create("(555) 1234"),
            SsnLast4Matches = true,
        };
}
