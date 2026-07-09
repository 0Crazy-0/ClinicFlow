using ClinicFlow.Application.AppointmentTypes.Commands.AddAllowedSpecialtyToAppointmentType;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.AddAllowedSpecialtyToAppointmentType;

public class AddAllowedSpecialtyToAppointmentTypeCommandValidatorTests
{
    private readonly AddAllowedSpecialtyToAppointmentTypeCommandValidator _sut;

    public AddAllowedSpecialtyToAppointmentTypeCommandValidatorTests()
    {
        _sut = new AddAllowedSpecialtyToAppointmentTypeCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new AddAllowedSpecialtyToAppointmentTypeCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
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
        var command = new AddAllowedSpecialtyToAppointmentTypeCommand(
            Guid.Empty,
            Guid.CreateVersion7()
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
        var command = new AddAllowedSpecialtyToAppointmentTypeCommand(
            Guid.CreateVersion7(),
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
