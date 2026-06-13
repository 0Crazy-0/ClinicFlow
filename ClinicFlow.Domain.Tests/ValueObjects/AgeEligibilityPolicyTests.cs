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

    [Fact]
    public void Create_ShouldCreateInstance_WhenAgesAreAllowedBoundaries()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(
            AgeEligibilityPolicy.MinimumAllowedAge,
            AgeEligibilityPolicy.MaximumAllowedAge,
            false
        );

        // Assert
        policy.MinimumAge.Should().Be(AgeEligibilityPolicy.MinimumAllowedAge);
        policy.MaximumAge.Should().Be(AgeEligibilityPolicy.MaximumAllowedAge);
    }

    [Fact]
    public void Create_ShouldCreateInstance_WhenMaximumAgeEqualsMinimumAllowedAge()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(
            null,
            AgeEligibilityPolicy.MinimumAllowedAge,
            false
        );

        // Assert
        policy.MaximumAge.Should().Be(AgeEligibilityPolicy.MinimumAllowedAge);
    }

    [Fact]
    public void Create_ShouldCreateInstance_WhenMinimumAgeEqualsMaximumAllowedAge()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(
            AgeEligibilityPolicy.MaximumAllowedAge,
            null,
            false
        );

        // Assert
        policy.MinimumAge.Should().Be(AgeEligibilityPolicy.MaximumAllowedAge);
    }

    [Fact]
    public void Create_ShouldCreateInstance_WhenMinimumAgeEqualsMaximumAge()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(18, 18, false);

        // Assert
        policy.MinimumAge.Should().Be(18);
        policy.MaximumAge.Should().Be(18);
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

    [Theory]
    [InlineData(-1, null)]
    [InlineData(null, -1)]
    public void Create_ShouldThrowException_WhenAgeIsNegative(int? minimumAge, int? maximumAge)
    {
        // Arrange & Act
        var act = () => AgeEligibilityPolicy.Create(minimumAge, maximumAge, false);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }

    [Theory]
    [InlineData(121, null)]
    [InlineData(null, 121)]
    public void Create_ShouldThrowException_WhenAgeExceedsMaximum(int? minimumAge, int? maximumAge)
    {
        // Arrange & Act
        var act = () => AgeEligibilityPolicy.Create(minimumAge, maximumAge, false);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueExceedsMaximum);
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

    [Theory]
    [InlineData(AgeEligibilityPolicy.MinimumAllowedAge)]
    [InlineData(AgeEligibilityPolicy.MaximumAllowedAge)]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenPatientAgeIsAllowedBoundary(
        int patientAge
    )
    {
        // Arrange
        var policy = AgeEligibilityPolicy.NoRestriction;

        // Act
        var act = () => policy.ValidatePatientEligibility(patientAge);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(18)]
    [InlineData(65)]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenPatientAgeEqualsPolicyBoundary(
        int patientAge
    )
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(18, 65, false);

        // Act
        var act = () => policy.ValidatePatientEligibility(patientAge);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenPatientAgeIsNegative()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.NoRestriction;

        // Act
        var act = () => policy.ValidatePatientEligibility(-1);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenPatientAgeExceedsMaximum()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.NoRestriction;

        // Act
        var act = () =>
            policy.ValidatePatientEligibility(AgeEligibilityPolicy.MaximumAllowedAge + 1);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueExceedsMaximum);
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

    [Fact]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenLegalGuardianRequiredAndPatientIsAdult()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(null, null, true);

        // Act
        var act = () => policy.ValidatePatientEligibility(18);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenLegalGuardianIsNotRequiredAndPatientIsUnderage()
    {
        // Arrange
        var policy = AgeEligibilityPolicy.Create(null, null, false);

        // Act
        var act = () => policy.ValidatePatientEligibility(16);

        // Assert
        act.Should().NotThrow();
    }
}
