using ClinicFlow.Application.AppointmentTypes.Commands.ChangeAppointmentTypeAgePolicy;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.ChangeAppointmentTypeAgePolicy;

public class ChangeAppointmentTypeAgePolicyCommandValidatorTests
{
    private readonly ChangeAppointmentTypeAgePolicyCommandValidator _sut;

    public ChangeAppointmentTypeAgePolicyCommandValidatorTests()
    {
        _sut = new ChangeAppointmentTypeAgePolicyCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new ChangeAppointmentTypeAgePolicyCommand(
            Guid.CreateVersion7(),
            18,
            65,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAgeFieldsAreNull()
    {
        // Arrange
        var command = new ChangeAppointmentTypeAgePolicyCommand(
            Guid.CreateVersion7(),
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new ChangeAppointmentTypeAgePolicyCommand(Guid.Empty, null, null, false);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenMinimumAgeIsNegative(int minimumAge)
    {
        // Arrange
        var command = new ChangeAppointmentTypeAgePolicyCommand(
            Guid.CreateVersion7(),
            minimumAge,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MinimumAge)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenMaximumAgeIsNegative(int maximumAge)
    {
        // Arrange
        var command = new ChangeAppointmentTypeAgePolicyCommand(
            Guid.CreateVersion7(),
            null,
            maximumAge,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaximumAge)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMinimumAgeExceedsMaximumAllowedAge()
    {
        // Arrange
        var command = new ChangeAppointmentTypeAgePolicyCommand(
            Guid.CreateVersion7(),
            AgeEligibilityPolicy.MaximumAllowedAge + 1,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MinimumAge)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMaximumAgeExceedsMaximumAllowedAge()
    {
        // Arrange
        var command = new ChangeAppointmentTypeAgePolicyCommand(
            Guid.CreateVersion7(),
            null,
            AgeEligibilityPolicy.MaximumAllowedAge + 1,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaximumAge)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }
}
