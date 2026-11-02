using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Common;

public class DomainRulesTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ComputeAge_ShouldReturnExactYears_WhenBirthdayPassed()
    {
        // Arrange
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var dateOfBirth = DateOnly.FromDateTime(
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).AddYears(-26)
        );

        // Act
        var age = DomainRules.ComputeAge(dateOfBirth, referenceDate);

        // Assert
        age.Should().Be(26);
    }

    [Fact]
    public void ComputeAge_ShouldReturnExactYears_WhenBirthdayNotYetPassed()
    {
        // Arrange
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var dateOfBirth = DateOnly.FromDateTime(
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).AddYears(-26)
        );

        // Act
        var age = DomainRules.ComputeAge(dateOfBirth, referenceDate);

        // Assert
        age.Should().Be(25);
    }

    [Fact]
    public void ComputeAge_ShouldReturnExactYears_OnTheBirthdayItself()
    {
        // Arrange
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-26));

        // Act
        var age = DomainRules.ComputeAge(dateOfBirth, referenceDate);

        // Assert
        age.Should().Be(26);
    }
}
