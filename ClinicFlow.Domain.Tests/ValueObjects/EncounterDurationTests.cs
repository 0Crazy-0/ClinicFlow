using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class EncounterDurationTests
{
    [Theory]
    [InlineData(10, true)]
    [InlineData(90, true)]
    [InlineData(30, true)]
    [InlineData(9, false)]
    [InlineData(91, false)]
    [InlineData(12, false)]
    public void IsValid_ShouldReturnExpectedResult(int minutes, bool expected)
    {
        // Act
        var result = EncounterDuration.IsValid(minutes);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    [InlineData(90)]
    public void FromMinutes_ShouldCreateEncounterDuration_WhenValueIsValid(int minutes)
    {
        // Act
        var duration = EncounterDuration.FromMinutes(minutes);

        // Assert
        duration.Minutes.Should().Be(minutes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(12)]
    [InlineData(91)]
    [InlineData(95)]
    [InlineData(-10)]
    public void FromMinutes_ShouldThrowException_WhenValueIsInvalid(int minutes)
    {
        // Act
        var act = () => EncounterDuration.FromMinutes(minutes);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
    }
}
