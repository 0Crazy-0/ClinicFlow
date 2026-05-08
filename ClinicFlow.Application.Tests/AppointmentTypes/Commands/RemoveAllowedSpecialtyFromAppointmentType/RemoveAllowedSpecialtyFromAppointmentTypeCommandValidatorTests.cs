using ClinicFlow.Application.AppointmentTypes.Commands.RemoveAllowedSpecialtyFromAppointmentType;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.RemoveAllowedSpecialtyFromAppointmentType;

public class RemoveAllowedSpecialtyFromAppointmentTypeCommandValidatorTests
{
    private readonly RemoveAllowedSpecialtyFromAppointmentTypeCommandValidator _sut;

    public RemoveAllowedSpecialtyFromAppointmentTypeCommandValidatorTests()
    {
        _sut = new RemoveAllowedSpecialtyFromAppointmentTypeCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new RemoveAllowedSpecialtyFromAppointmentTypeCommand(
            Guid.NewGuid(),
            Guid.NewGuid()
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
        var command = new RemoveAllowedSpecialtyFromAppointmentTypeCommand(
            Guid.Empty,
            Guid.NewGuid()
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSpecialtyIdIsEmpty()
    {
        // Arrange
        var command = new RemoveAllowedSpecialtyFromAppointmentTypeCommand(
            Guid.NewGuid(),
            Guid.Empty
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SpecialtyId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
