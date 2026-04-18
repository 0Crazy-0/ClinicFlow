using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class AgeEligibilityPolicyTests
{
    [Fact]
    public void Create_ShouldCreateInstance_WhenAgeRangeIsValid()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(18, 65, true);

        // Assert
        policy.Should().NotBeNull();
        policy.MinimumAge.Should().Be(18);
        policy.MaximumAge.Should().Be(65);
        policy.RequiresLegalGuardian.Should().BeTrue();
    }

    [Fact]
    public void NoRestriction_ShouldReturnPolicyWithNullAgesAndNoGuardianRequired()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.NoRestriction;

        // Assert
        policy.Should().NotBeNull();
        policy.MinimumAge.Should().BeNull();
        policy.MaximumAge.Should().BeNull();
        policy.RequiresLegalGuardian.Should().BeFalse();
    }

    [Theory]
    [InlineData(10, 5)]
    [InlineData(18, 17)]
    public void Create_ShouldThrowException_WhenMinimumAgeIsGreaterThanMaximumAge(
        int minAge,
        int maxAge
    )
    {
        // Arrange & Act
        var act = () => AgeEligibilityPolicy.Create(minAge, maxAge, false);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.InvalidAgeRange);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenPatientMeetsAllCriteria()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(18, 65, false);

        // Act
        var act = () => policy.ValidatePatientEligibility(30);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenPatientIsTooYoung()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(18, null, false);

        // Act
        var act = () => policy.ValidatePatientEligibility(15);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MinimumAgeNotMet);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenPatientIsTooOld()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(null, 14, false);

        // Act
        var act = () => policy.ValidatePatientEligibility(30);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MaximumAgeExceeded);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenLegalGuardianRequiredAndPatientIsUnderage()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(null, null, true);

        // Act
        var act = () => policy.ValidatePatientEligibility(16);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.LegalGuardianRequired);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenLegalGuardianRequiredAndPatientIsUnderage_ButHasConsent()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(null, null, true);

        // Act
        var act = () => policy.ValidatePatientEligibility(16, true);

        // Assert
        act.Should().NotThrow();
    }
}
