using ClinicFlow.Application.AppointmentTypes.Commands.DeactivateAppointmentType;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.DeactivateAppointmentType;

public class DeactivateAppointmentTypeCommandValidatorTests
{
    private readonly DeactivateAppointmentTypeCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new DeactivateAppointmentTypeCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new DeactivateAppointmentTypeCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
