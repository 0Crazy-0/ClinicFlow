using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class TimeRangeTests
{
    // Constructor
    [Fact]
    public void Constructor_ShouldThrowException_WhenStartIsAfterEnd()
    {
        // Arrange & Act
        var act = () => new TimeRange(TimeSpan.FromHours(10), TimeSpan.FromHours(9));

        // Assert
        act.Should().Throw<InvalidTimeRangeException>().WithMessage("Start time must be before end time.");
    }

    // Duration
    [Fact]
    public void Duration_ShouldReturnCorrectDifference()
    {
        // Arrange
        var timeRange = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10.5)); // 1.5 hours

        // Act & Assert
        timeRange.Duration.Should().Be(TimeSpan.FromMinutes(90));
    }

    // Equality
    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var tr1 = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));
        var tr2 = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));
        var tr3 = new TimeRange(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        // Act & Assert
        (tr1 == tr2).Should().BeTrue();
        (tr1 != tr3).Should().BeTrue();
    }

    // OverlapsWith
    [Fact]
    public void OverlapsWith_ShouldThrowException_WhenOtherIsNull()
    {
        // Arrange
        var range = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        // Act
        var act = () => range.OverlapsWith(null!);

        // Assert
        act.Should().Throw<InvalidTimeRangeException>().WithMessage("Time range cannot be null.");
    }

    [Theory]
    [InlineData(9, 10, 8, 9, false)] // Before, no overlap
    [InlineData(9, 10, 8, 9.5, true)] // Partial overlap start
    [InlineData(9, 10, 9.5, 10.5, true)] // Partial overlap end
    [InlineData(9, 10, 10, 11, false)] // After, no overlap (touching)
    [InlineData(9, 10, 9.2, 9.8, true)] // Inside
    [InlineData(9, 10, 8, 11, true)] // Enveloping (other covers range)
    public void OverlapsWith_ShouldReturnCorrectResult(double start, double end, double otherStart, double otherEnd, bool expected)
    {
        // Arrange
        var range = new TimeRange(TimeSpan.FromHours(start), TimeSpan.FromHours(end));

        // Act & Assert
        range.OverlapsWith(new TimeRange(TimeSpan.FromHours(otherStart), TimeSpan.FromHours(otherEnd))).Should().Be(expected);
    }

    // Covers
    [Fact]
    public void Covers_ShouldThrowException_WhenOtherIsNull()
    {
        // Arrange
        var range = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        // Act
        var act = () => range.Covers(null!);

        // Assert
        act.Should().Throw<InvalidTimeRangeException>().WithMessage("Time range cannot be null.");
    }

    [Theory]
    [InlineData(9, 10, 9, 10, true)] // Exact match
    [InlineData(9, 10, 9.5, 9.8, true)] // Inside
    [InlineData(9, 10, 8, 11, false)] // Enveloped by other (so range does NOT cover other)
    [InlineData(9, 10, 8, 9, false)] // Outside before
    [InlineData(9, 10, 10, 11, false)] // Outside after
    [InlineData(9, 10, 9, 10.1, false)] // Slightly extending end
    [InlineData(9, 10, 8.9, 10, false)] // Slightly extending start
    public void Covers_ShouldReturnCorrectResult(double start, double end, double otherStart, double otherEnd, bool expected)
    {
        // Arrange
        var range = new TimeRange(TimeSpan.FromHours(start), TimeSpan.FromHours(end));

        // Act & Assert
        range.Covers(new TimeRange(TimeSpan.FromHours(otherStart), TimeSpan.FromHours(otherEnd))).Should().Be(expected);
    }
}
