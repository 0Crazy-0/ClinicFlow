using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class TimeRangeTests
{
    [Fact]
    public void Create_ShouldCreateTimeRange_WhenStartIsBeforeEnd()
    {
        // Arrange
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(17, 0);

        // Act
        var timeRange = TimeRange.Create(start, end);

        // Assert
        timeRange.Start.Should().Be(start);
        timeRange.End.Should().Be(end);
    }

    [Fact]
    public void Create_ShouldThrowInvalidTimeRangeException_WhenStartEqualsEnd()
    {
        // Arrange
        var time = new TimeOnly(9);

        // Act
        var act = () => TimeRange.Create(time, time);

        // Assert
        act.Should()
            .Throw<InvalidTimeRangeException>()
            .WithMessage(DomainErrors.Schedule.InvalidTimeRange);
    }

    [Fact]
    public void Create_ShouldThrowInvalidTimeRangeException_WhenStartIsAfterEnd()
    {
        // Arrange
        var start = new TimeOnly(17);
        var end = new TimeOnly(9);

        // Act
        var act = () => TimeRange.Create(start, end);

        // Assert
        act.Should()
            .Throw<InvalidTimeRangeException>()
            .WithMessage(DomainErrors.Schedule.InvalidTimeRange);
    }

    [Fact]
    public void Covers_ShouldThrowDomainValidationException_WhenOtherIsNull()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));

        // Act
        var act = () => timeRange.Covers(null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Covers_ShouldReturnTrue_WhenOtherMatchesRangeExactly()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Covers_ShouldReturnTrue_WhenOtherIsStrictlyWithinRange()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(10), new TimeOnly(16));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Covers_ShouldReturnTrue_WhenOtherHasSameStartAndEarlierEnd()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(9), new TimeOnly(12));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Covers_ShouldReturnTrue_WhenOtherHasLaterStartAndSameEnd()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(12), new TimeOnly(17));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Covers_ShouldReturnFalse_WhenOtherStartsBeforeRangeAndEndsWithinRange()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(8), new TimeOnly(12));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Covers_ShouldReturnFalse_WhenOtherStartsWithinRangeAndEndsAfterRange()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(12), new TimeOnly(18));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Covers_ShouldReturnFalse_WhenOtherStartsBeforeRangeAndEndsAfterRange()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(8), new TimeOnly(18));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Covers_ShouldReturnFalse_WhenOtherIsCompletelyBeforeRange()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(7), new TimeOnly(8));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Covers_ShouldReturnFalse_WhenOtherIsCompletelyAfterRange()
    {
        // Arrange
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(17));
        var other = TimeRange.Create(new TimeOnly(18), new TimeOnly(19));

        // Act
        var result = timeRange.Covers(other);

        // Assert
        result.Should().BeFalse();
    }
}
