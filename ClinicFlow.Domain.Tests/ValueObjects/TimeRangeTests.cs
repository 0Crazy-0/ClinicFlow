using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class TimeRangeTests
{
    [Fact]
    public void Constructor_ShouldThrowException_WhenStartIsAfterEnd()
    {
        // Arrange & Act
        var act = () => TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(9));

        // Assert
        act.Should()
            .Throw<InvalidTimeRangeException>()
            .WithMessage(DomainErrors.Schedule.InvalidTimeRange);
    }

    [Fact]
    public void Duration_ShouldReturnCorrectDifference()
    {
        // Arrange
        var timeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10.5)); // 1.5 hours

        // Act & Assert
        timeRange.Duration.Should().Be(TimeSpan.FromMinutes(90));
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var tr1 = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));
        var tr2 = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));
        var tr3 = TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        // Act & Assert
        (tr1 == tr2)
            .Should()
            .BeTrue();
        (tr1 != tr3).Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_ShouldThrowException_WhenOtherIsNull()
    {
        // Arrange
        var range = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

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
        var range = TimeRange.Create(TimeSpan.FromHours(start), TimeSpan.FromHours(end));

        // Act & Assert
        range
            .OverlapsWith(
                TimeRange.Create(TimeSpan.FromHours(otherStart), TimeSpan.FromHours(otherEnd))
            )
            .Should()
            .Be(expected);
    }

    [Fact]
    public void Covers_ShouldThrowException_WhenOtherIsNull()
    {
        // Arrange
        var range = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

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
        var range = TimeRange.Create(TimeSpan.FromHours(start), TimeSpan.FromHours(end));

        // Act & Assert
        range
            .Covers(TimeRange.Create(TimeSpan.FromHours(otherStart), TimeSpan.FromHours(otherEnd)))
            .Should()
            .Be(expected);
    }
}
