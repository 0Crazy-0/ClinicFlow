using AwesomeAssertions;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.Services.Registration;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Services.Registration;

public class MinorPatientDuplicateScoringServiceTests
{
    [Fact]
    public void Score_ShouldReturnEmptyScores_WhenCandidateListIsEmpty()
    {
        // Arrange
        var form = CreateStrangerForm();
        var context = new PatientDuplicateScoringContext { Candidates = [] };

        // Act
        var result = MinorPatientDuplicateScoringService.Score(form, context);

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
        var result = MinorPatientDuplicateScoringService.Score(form, context);

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
        var result = MinorPatientDuplicateScoringService.Score(form, context);

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
        var result = MinorPatientDuplicateScoringService.Score(form, context);

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
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(10);
    }

    [Fact]
    public void Score_ShouldScoreSurnameSimilarity_WhenSurnamesHaveEditDistanceTwo()
    {
        // Arrange
        // canonicalized "garcia" and "garza" have edit distance 2 of a maximum 6,
        // so the surname contribution is 20 * (1 - 2/6) = 13.333... rounded away from zero to 13.
        var form = CreateStrangerForm() with { FirstSurname = "Garcia" };

        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("Juan", null, "Garza"),
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(13);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlySexAtBirthMatches()
    {
        // Arrange
        var form = CreateStrangerForm() with { SexAtBirth = BiologicalSex.Female };

        var candidate = CreateStrangerCandidate();
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(5);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlySsnLast4Matches()
    {
        // Arrange
        var form = CreateStrangerForm() with { HasSsnLast4 = true };

        var candidate = CreateStrangerCandidate() with { SsnLast4Matches = true };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(20);
    }

    [Fact]
    public void Score_ShouldNotCountSsnLast4_WhenFormLacksSsnLast4()
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
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldReturnExactScore_WhenOnlyHomeAddressMatches()
    {
        // Arrange
        var form = CreateStrangerForm();
        var candidate = CreateStrangerCandidate() with { HomeAddress = form.HomeAddress };
        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(5);
    }

    [Fact]
    public void Score_ShouldReturnZeroAddressWeight_WhenStateOrPostalCodeDiffer()
    {
        // Arrange
        // the address line is identical, but the state differs, so the address
        // contributes nothing and no other dimension matches.
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
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldReturnMaxScore_WhenCandidateMatchesAllDimensions()
    {
        // Arrange
        // the minor form carries no contact data, so the maximum reachable score
        // is 90: date of birth 30, surname 20, given name 10, ssn 20, sex 5, address 5.
        var form = CreateStrangerForm() with { HasSsnLast4 = true };

        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("Remy", null, "Levy"),
            DateOfBirth = form.DateOfBirth,
            SexAtBirth = BiologicalSex.Male,
            HomeAddress = form.HomeAddress,
            SsnLast4Matches = true,
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(90);
    }

    [Fact]
    public void Score_ShouldMarkHighBand_WhenCombinedIdentityScoreReachesThreshold()
    {
        // Arrange
        // date of birth 30, surname 20, given name 10 and ssn 20 total 80,
        // which crosses the 70 threshold without any contact dimension.
        var form = CreateStrangerForm() with { HasSsnLast4 = true };

        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("Remy", null, "Levy"),
            DateOfBirth = form.DateOfBirth,
            SsnLast4Matches = true,
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(80);
        result.IsHighBand.Should().BeTrue();
    }

    [Fact]
    public void Score_ShouldNotMarkHighBand_WhenCombinedIdentityScoreStaysBelowThreshold()
    {
        // Arrange
        // date of birth 30, surname 20, given name 10 and sex 5 total 65,
        // which stays below the 70 threshold.
        var form = CreateStrangerForm();

        var candidate = CreateStrangerCandidate() with
        {
            FullName = PersonName.Create("Remy", null, "Levy"),
            DateOfBirth = form.DateOfBirth,
            SexAtBirth = BiologicalSex.Male,
        };

        var context = new PatientDuplicateScoringContext { Candidates = [candidate] };

        // Act
        var result = MinorPatientDuplicateScoringService.Score(form, context);

        // Assert
        result.Scores.Should().ContainSingle().Which.Score.Should().Be(65);
        result.IsHighBand.Should().BeFalse();
    }

    [Fact]
    public void Score_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () => MinorPatientDuplicateScoringService.Score(null!, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Score_ShouldThrowArgumentNullException_WhenHomeAddressIsNull()
    {
        // Arrange
        var form = CreateStrangerForm() with { HomeAddress = null! };

        // Act
        var act = () => MinorPatientDuplicateScoringService.Score(form, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Score_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () => MinorPatientDuplicateScoringService.Score(CreateStrangerForm(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static PatientDuplicateScoringContext CreateValidContext() =>
        new() { Candidates = [CreateStrangerCandidate()] };

    private static MinorPatientDuplicateScoringArgs CreateStrangerForm() =>
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
}
