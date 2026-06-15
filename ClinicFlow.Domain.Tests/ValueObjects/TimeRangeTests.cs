using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class TimeRangeTests
{
    [Fact]
    public void Constructor_ShouldThrowException_WhenStartIsAfterEnd()
    {
        // Arrange & Act
        var act = () => TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(9, 0));

        // Assert
        act.Should()
            .Throw<InvalidTimeRangeException>()
            .WithMessage(DomainErrors.Schedule.InvalidTimeRange);
    }

    [Fact]
    public void Duration_ShouldReturnCorrectDifference()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 30)); // 1.5 hours

        // Assert
        timeRange.Duration.Should().Be(TimeSpan.FromMinutes(90));
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var tr1 = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var tr2 = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var tr3 = TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0));

        // Assert
        (tr1 == tr2)
            .Should()
            .BeTrue();
        (tr1 != tr3).Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_ShouldThrowException_WhenOtherIsNull()
    {
        // Arrange
        var range = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));

        // Act
        var act = () => range.OverlapsWith(null!);

        // Assert
        act.Should()
            .Throw<InvalidTimeRangeException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Theory]
    [InlineData(9, 10, 8, 9, false)] // Before, no overlap
    [InlineData(9, 10, 8, 9.5, true)] // Partial overlap start
    [InlineData(9, 10, 9.5, 10.5, true)] // Partial overlap end
    [InlineData(9, 10, 10, 11, false)] // After, no overlap (touching)
    [InlineData(9, 10, 9.2, 9.8, true)] // Inside
    [InlineData(9, 10, 8, 11, true)] // Enveloping (other covers range)
    public void OverlapsWith_ShouldReturnCorrectResult(
        double start,
        double end,
        double otherStart,
        double otherEnd,
        bool expected
    )
    {
        // Arrange
        var range = TimeRange.Create(
            TimeOnly.MinValue.AddHours(start),
            TimeOnly.MinValue.AddHours(end)
        );

        // Act & Assert
        range
            .OverlapsWith(
                TimeRange.Create(
                    TimeOnly.MinValue.AddHours(otherStart),
                    TimeOnly.MinValue.AddHours(otherEnd)
                )
            )
            .Should()
            .Be(expected);
    }

    [Fact]
    public void Covers_ShouldThrowException_WhenOtherIsNull()
    {
        // Arrange
        var range = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));

        // Act
        var act = () => range.Covers(null!);

        // Assert
        act.Should()
            .Throw<InvalidTimeRangeException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Theory]
    [InlineData(9, 10, 9, 10, true)] // Exact match
    [InlineData(9, 10, 9.5, 9.8, true)] // Inside
    [InlineData(9, 10, 8, 11, false)] // Enveloped by other (so range does NOT cover other)
    [InlineData(9, 10, 8, 9, false)] // Outside before
    [InlineData(9, 10, 10, 11, false)] // Outside after
    [InlineData(9, 10, 9, 10.1, false)] // Slightly extending end
    [InlineData(9, 10, 8.9, 10, false)] // Slightly extending start
    public void Covers_ShouldReturnCorrectResult(
        double start,
        double end,
        double otherStart,
        double otherEnd,
        bool expected
    )
    {
        // Arrange
        var range = TimeRange.Create(
            TimeOnly.MinValue.AddHours(start),
            TimeOnly.MinValue.AddHours(end)
        );

        // Act & Assert
        range
            .Covers(
                TimeRange.Create(
                    TimeOnly.MinValue.AddHours(otherStart),
                    TimeOnly.MinValue.AddHours(otherEnd)
                )
            )
            .Should()
            .Be(expected);
    }
}
