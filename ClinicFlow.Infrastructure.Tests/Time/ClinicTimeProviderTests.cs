using System.Globalization;
using AwesomeAssertions;
using ClinicFlow.Infrastructure.Time;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Infrastructure.Tests.Time;

public class ClinicTimeProviderTests
{
    private readonly ClinicOptions _clinicOptions = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ClinicTimeProvider _sut;

    public ClinicTimeProviderTests()
    {
        _sut = new ClinicTimeProvider(_fakeTime, _clinicOptions.TimeZoneId);
    }

    [Fact]
    public void Constructor_ShouldThrowTimeZoneNotFoundException_WhenTimeZoneIdIsInvalid()
    {
        // Arrange
        var act = () => new ClinicTimeProvider(_fakeTime, "Invalid/Timezone");

        // Act & Assert
        act.Should().Throw<TimeZoneNotFoundException>();
    }

    [Fact]
    public void GetUtcNow_ShouldDelegateToInnerProvider()
    {
        // Arrange
        var instant = DateTimeOffset.Parse("2026-01-15T12:00:00Z", CultureInfo.InvariantCulture);
        _fakeTime.SetUtcNow(instant);

        // Act
        var utcNow = _sut.GetUtcNow();

        // Assert
        utcNow.Should().Be(instant);
    }

    [Fact]
    public void LocalTimeZone_ShouldReturnConfiguredClinicZone()
    {
        // Act
        var localTimeZone = _sut.LocalTimeZone;

        // Assert
        localTimeZone.Should().Be(TimeZoneInfo.FindSystemTimeZoneById(_clinicOptions.TimeZoneId));
    }

    [Fact]
    public void GetLocalNow_ShouldApplyStandardTimeOffset_WhenInstantFallsInWinter()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-01-15T12:00:00Z", CultureInfo.InvariantCulture)
        );

        // Act
        var localNow = _sut.GetLocalNow();

        // Assert
        localNow
            .DateTime.Should()
            .Be(DateTime.Parse("2026-01-15T04:00:00", CultureInfo.InvariantCulture));
        localNow.Offset.Should().Be(TimeSpan.FromHours(-8));
    }

    [Fact]
    public void GetLocalNow_ShouldApplyDaylightTimeOffset_WhenInstantFallsInSummer()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-07-15T12:00:00Z", CultureInfo.InvariantCulture)
        );

        // Act
        var localNow = _sut.GetLocalNow();

        // Assert
        localNow
            .DateTime.Should()
            .Be(DateTime.Parse("2026-07-15T05:00:00", CultureInfo.InvariantCulture));
        localNow.Offset.Should().Be(TimeSpan.FromHours(-7));
    }

    [Fact]
    public void GetLocalNow_ShouldAdvanceWithInnerProviderTime()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-01-15T12:00:00Z", CultureInfo.InvariantCulture)
        );
        var initialLocalNow = _sut.GetLocalNow();
        _fakeTime.Advance(TimeSpan.FromHours(1));

        // Act
        var advancedLocalNow = _sut.GetLocalNow();

        // Assert
        advancedLocalNow.Should().Be(initialLocalNow + TimeSpan.FromHours(1));
    }
}
