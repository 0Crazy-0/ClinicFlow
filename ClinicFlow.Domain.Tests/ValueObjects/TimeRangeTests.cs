using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class TimeRangeTests
{
// Constructor
    [Fact]
    public void Constructor_ShouldThrowException_WhenStartIsAfterEnd()
    {
        // Arrange
        var start = TimeSpan.FromHours(10);
        var end = TimeSpan.FromHours(9);

        // Act
        var act = () => new TimeRange(start, end);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Start time must be before end time.");
    }

    // Duration
    [Fact]
    public void Duration_ShouldReturnCorrectDifference()
    {
        // Arrange
        var start = TimeSpan.FromHours(9);
        var end = TimeSpan.FromHours(10.5); // 1.5 hours
        var timeRange = new TimeRange(start, end);

        // Act
        var duration = timeRange.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromMinutes(90));
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
}
