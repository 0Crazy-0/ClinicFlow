using System.Globalization;
using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Policies;

public class DaylightTransitionPolicyTests
{
    private static readonly TimeZoneInfo ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
        "America/Los_Angeles"
    );
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void EnsureSchedulable_ShouldNotThrow_WhenSlotIsUnambiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );
        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var slot = TimeRange.Create(new TimeOnly(10), new TimeOnly(11));

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, slot, ClinicTimeZone);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureSchedulable_ShouldThrowArgumentNullException_WhenSlotIsNull()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, null!, ClinicTimeZone);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureSchedulable_ShouldThrowArgumentNullException_WhenClinicTimeZoneIsNull()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );
        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var slot = TimeRange.Create(new TimeOnly(10), new TimeOnly(11));

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, slot, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureSchedulable_ShouldThrowBusinessRuleValidationException_WhenStartIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );
        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var slot = TimeRange.Create(new TimeOnly(1, 30), new TimeOnly(2, 30));

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, slot, ClinicTimeZone);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }

    [Fact]
    public void EnsureSchedulable_ShouldThrowBusinessRuleValidationException_WhenEndIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );
        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var slot = TimeRange.Create(new TimeOnly(0, 30), new TimeOnly(1, 30));

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, slot, ClinicTimeZone);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }

    [Fact]
    public void EnsureSchedulable_ShouldThrowBusinessRuleValidationException_WhenSlotIsInvalid()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-03-08T12:00:00Z", CultureInfo.InvariantCulture)
        );
        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var slot = TimeRange.Create(new TimeOnly(2, 30), new TimeOnly(3, 30));

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, slot, ClinicTimeZone);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.InvalidTimeSlot);
    }

    [Fact]
    public void EnsureSchedulable_ShouldThrowBusinessRuleValidationException_WhenEndIsInvalid()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-03-08T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var slot = TimeRange.Create(new TimeOnly(1, 30), new TimeOnly(2, 30));

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, slot, ClinicTimeZone);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.InvalidTimeSlot);
    }

    [Fact]
    public void EnsureSchedulable_ShouldThrowBusinessRuleValidationException_WhenSlotSpansSpringForwardGap()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-03-08T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var slot = TimeRange.Create(new TimeOnly(1, 30), new TimeOnly(3, 30));

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, slot, ClinicTimeZone);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.InvalidTimeSlot);
    }

    [Fact]
    public void EnsureSchedulable_ShouldThrowBusinessRuleValidationException_WhenSlotSpansFallBackOverlap()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var slot = TimeRange.Create(new TimeOnly(0, 30), new TimeOnly(2, 30));

        // Act
        var act = () => DaylightTransitionPolicy.EnsureSchedulable(date, slot, ClinicTimeZone);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }
}
