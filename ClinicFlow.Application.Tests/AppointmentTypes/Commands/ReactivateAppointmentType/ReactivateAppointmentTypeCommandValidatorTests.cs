using ClinicFlow.Application.AppointmentTypes.Commands.ReactivateAppointmentType;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.ReactivateAppointmentType;

public class ReactivateAppointmentTypeCommandValidatorTests
{
    private readonly ReactivateAppointmentTypeCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new ReactivateAppointmentTypeCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new ReactivateAppointmentTypeCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
