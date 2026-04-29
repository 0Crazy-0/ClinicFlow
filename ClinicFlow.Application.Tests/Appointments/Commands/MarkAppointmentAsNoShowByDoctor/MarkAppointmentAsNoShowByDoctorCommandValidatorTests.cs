using ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByDoctor;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.MarkAppointmentAsNoShowByDoctor;

public class MarkAppointmentAsNoShowByDoctorCommandValidatorTests
{
    private readonly MarkAppointmentAsNoShowByDoctorCommandValidator _sut;

    public MarkAppointmentAsNoShowByDoctorCommandValidatorTests()
    {
        _sut = new MarkAppointmentAsNoShowByDoctorCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByDoctorCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByDoctorCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByDoctorCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
