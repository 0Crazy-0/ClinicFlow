using ClinicFlow.Application.AppointmentTypes.Commands.MakeAppointmentTypeUnrestricted;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.MakeAppointmentTypeUnrestricted;

public class MakeAppointmentTypeUnrestrictedCommandValidatorTests
{
    private readonly MakeAppointmentTypeUnrestrictedCommandValidator _sut;

    public MakeAppointmentTypeUnrestrictedCommandValidatorTests()
    {
        _sut = new MakeAppointmentTypeUnrestrictedCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAppointmentTypeIdIsProvided()
    {
        // Arrange
        var command = new MakeAppointmentTypeUnrestrictedCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new MakeAppointmentTypeUnrestrictedCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
