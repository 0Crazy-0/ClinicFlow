using AwesomeAssertions;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.Services.Results.Registration;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Services.Registration;

public class PatientDuplicateScoringResultTests
{
    [Fact]
    public void MaxScore_ShouldReturnZero_WhenScoresIsEmpty()
    {
        // Arrange
        var result = new PatientDuplicateScoringResult([]);

        // Act & Assert
        result.MaxScore.Should().Be(0);
    }

    [Fact]
    public void MaxScore_ShouldReturnHighestScore_WhenMultipleScoresExist()
    {
        // Arrange
        var result = CreateScoringResult(30, 90, 10);

        // Act & Assert
        result.MaxScore.Should().Be(90);
    }

    [Fact]
    public void IsHighBand_ShouldBeTrue_WhenMaxScoreEqualsThreshold()
    {
        // Arrange
        var result = CreateScoringResult(PatientDuplicateScoringService.HighScoreThreshold);

        // Act & Assert
        result.IsHighBand.Should().BeTrue();
    }

    [Fact]
    public void IsHighBand_ShouldBeTrue_WhenMaxScoreExceedsThreshold()
    {
        // Arrange
        var result = CreateScoringResult(PatientDuplicateScoringService.HighScoreThreshold + 1);

        // Act & Assert
        result.IsHighBand.Should().BeTrue();
    }

    [Fact]
    public void IsHighBand_ShouldBeFalse_WhenMaxScoreIsBelowThreshold()
    {
        // Arrange
        var result = CreateScoringResult(PatientDuplicateScoringService.HighScoreThreshold - 1);

        // Act & Assert
        result.IsHighBand.Should().BeFalse();
    }

    private static PatientDuplicateScoringResult CreateScoringResult(params int[] scores) =>
        new([.. scores.Select(score => new PatientDuplicateScore(CreateCandidate(), score))]);

    private static PatientDuplicateCandidate CreateCandidate() =>
        new()
        {
            PatientId = Guid.CreateVersion7(),
            FullName = PersonName.Create("Twilight", null, "Sparkle"),
            DateOfBirth = new DateOnly(2010, 10, 10),
            SexAtBirth = BiologicalSex.Male,
            HomeAddress = Address.Create(
                "The castle of friendship",
                AddressState.Florida,
                "33101",
                Country.UnitedStates
            ),
        };
}
