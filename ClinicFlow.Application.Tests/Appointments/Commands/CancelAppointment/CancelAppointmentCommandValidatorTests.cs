using ClinicFlow.Application.Appointments.Commands.CancelAppointment;
using FluentAssertions;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandValidatorTests
{
    private readonly CancelAppointmentCommandValidator _sut;

    public CancelAppointmentCommandValidatorTests()
    {
        _sut = new CancelAppointmentCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvided()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), false, "Reason");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.Empty, Guid.NewGuid(), false, "Reason");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.AppointmentId));
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.Empty, false, "Reason");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.InitiatorUserId));
    }
}
