using ClinicFlow.Application.AppointmentTypes.Commands.DeleteAppointmentType;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.DeleteAppointmentType;

public class DeleteAppointmentTypeCommandValidatorTests
{
    private readonly DeleteAppointmentTypeCommandValidator _sut;

    public DeleteAppointmentTypeCommandValidatorTests()
    {
        _sut = new DeleteAppointmentTypeCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAppointmentTypeIdIsProvided()
    {
        // Arrange
        var command = new DeleteAppointmentTypeCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new DeleteAppointmentTypeCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
