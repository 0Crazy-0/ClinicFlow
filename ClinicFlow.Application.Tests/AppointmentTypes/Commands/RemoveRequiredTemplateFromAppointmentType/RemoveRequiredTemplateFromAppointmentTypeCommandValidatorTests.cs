using ClinicFlow.Application.AppointmentTypes.Commands.RemoveRequiredTemplateFromAppointmentType;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.RemoveRequiredTemplateFromAppointmentType;

public class RemoveRequiredTemplateFromAppointmentTypeCommandValidatorTests
{
    private readonly RemoveRequiredTemplateFromAppointmentTypeCommandValidator _sut;

    public RemoveRequiredTemplateFromAppointmentTypeCommandValidatorTests()
    {
        _sut = new RemoveRequiredTemplateFromAppointmentTypeCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new RemoveRequiredTemplateFromAppointmentTypeCommand(
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
        var command = new RemoveRequiredTemplateFromAppointmentTypeCommand(
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
    public void Validate_ShouldHaveError_WhenTemplateIdIsEmpty()
    {
        // Arrange
        var command = new RemoveRequiredTemplateFromAppointmentTypeCommand(
            Guid.CreateVersion7(),
            Guid.Empty
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
