using ClinicFlow.Application.AppointmentTypes.Commands.AddRequiredTemplateToAppointmentType;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.AddRequiredTemplateToAppointmentType;

public class AddRequiredTemplateToAppointmentTypeCommandValidatorTests
{
    private readonly AddRequiredTemplateToAppointmentTypeCommandValidator _sut;

    public AddRequiredTemplateToAppointmentTypeCommandValidatorTests()
    {
        _sut = new AddRequiredTemplateToAppointmentTypeCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new AddRequiredTemplateToAppointmentTypeCommand(
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
        var command = new AddRequiredTemplateToAppointmentTypeCommand(Guid.Empty, Guid.NewGuid());

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
        var command = new AddRequiredTemplateToAppointmentTypeCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
