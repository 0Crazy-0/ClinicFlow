using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class CancellationLimitTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(48)]
    [InlineData(72)]
    public void FromHours_ShouldCreateCancellationLimit_WhenValueIsAllowed(int hours)
    {
        // Act
        var limit = CancellationLimit.FromHours(hours);

        // Assert
        limit.Hours.Should().Be(hours);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(23)]
    [InlineData(49)]
    [InlineData(71)]
    [InlineData(100)]
    public void FromHours_ShouldThrowException_WhenValueIsNotAllowed(int hours)
    {
        // Act
        var act = () => CancellationLimit.FromHours(hours);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidCancellationLimit);
    }

    [Fact]
    public void IsNoticePeriodMet_ShouldReturnTrue_WhenSufficientNotice()
    {
        // Arrange
        var limit = CancellationLimit.FromHours(24);
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var appointmentDateTime = referenceTime.AddHours(48);

        // Act
        var result = limit.IsNoticePeriodMet(appointmentDateTime, referenceTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNoticePeriodMet_ShouldReturnFalse_WhenInsufficientNotice()
    {
        // Arrange
        var limit = CancellationLimit.FromHours(24);
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var appointmentDateTime = referenceTime.AddHours(12);

        // Act
        var result = limit.IsNoticePeriodMet(appointmentDateTime, referenceTime);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNoticePeriodMet_ShouldReturnTrue_WhenExactNotice()
    {
        // Arrange
        var limit = CancellationLimit.FromHours(24);
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var appointmentDateTime = referenceTime.AddHours(24);

        // Act
        var result = limit.IsNoticePeriodMet(appointmentDateTime, referenceTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNoticePeriodMet_ShouldReturnTrue_WhenHoursIsZero()
    {
        // Arrange
        var limit = CancellationLimit.FromHours(0);
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var appointmentDateTime = referenceTime.AddMinutes(1);

        // Act
        var result = limit.IsNoticePeriodMet(appointmentDateTime, referenceTime);

        // Assert
        result.Should().BeTrue();
    }
}
