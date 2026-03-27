using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentTypeDefinitionTests
{
    [Fact]
    public void Create_ShouldCreateInstance_WhenValidParameters()
    {
        // Arrange
        var type = AppointmentCategory.Checkup;
        var name = "General Checkup";
        var description = "Routine consultation";
        var duration = TimeSpan.FromMinutes(30);

        // Act
        var result = AppointmentTypeDefinition.Create(type, name, description, duration);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(type);
        result.Name.Should().Be(name);
        result.Description.Should().Be(description);
        result.DurationMinutes.Should().Be(duration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange & Act
        var act = () =>
            AppointmentTypeDefinition.Create(
                AppointmentCategory.Checkup,
                name!,
                "Description",
                TimeSpan.FromMinutes(30)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [MemberData(nameof(InvalidDurations))]
    public void Create_ShouldThrowException_WhenDurationIsZeroOrNegative(TimeSpan duration)
    {
        // Arrange & Act
        var act = () =>
            AppointmentTypeDefinition.Create(
                AppointmentCategory.Checkup,
                "Checkup",
                "Description",
                duration
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    public static TheoryData<TimeSpan> InvalidDurations =>
        [TimeSpan.Zero, TimeSpan.FromMinutes(-10)];

    [Theory]
    [InlineData(10, 5)]
    [InlineData(18, 17)]
    public void AgeEligibilityPolicy_Create_ShouldThrowException_WhenMinimumAgeIsGreaterThanMaximumAge(
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
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Adult Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, 65, false)
        );

        // Act
        var act = () => appointmentType.ValidatePatientEligibility(30);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenPatientIsTooYoung()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Adult Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, null, false)
        );

        // Act
        var act = () => appointmentType.ValidatePatientEligibility(15);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MinimumAgeNotMet);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenPatientIsTooOld()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Pediatric Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(null, 14, false)
        );

        // Act
        var act = () => appointmentType.ValidatePatientEligibility(30);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MaximumAgeExceeded);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldThrowException_WhenLegalGuardianRequiredAndPatientIsUnderage()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Surgery",
            "Description",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(null, null, true)
        );

        // Act
        var act = () => appointmentType.ValidatePatientEligibility(16);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.LegalGuardianRequired);
    }

    [Fact]
    public void ValidatePatientEligibility_ShouldNotThrowException_WhenLegalGuardianRequiredAndPatientIsUnderage_ButHasConsent()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Surgery",
            "Description",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(null, null, true)
        );

        // Act
        var act = () => appointmentType.ValidatePatientEligibility(16, hasGuardianConsent: true);

        // Assert
        act.Should().NotThrow();
    }
}
