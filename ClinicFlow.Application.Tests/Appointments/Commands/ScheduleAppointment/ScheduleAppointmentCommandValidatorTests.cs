using ClinicFlow.Application.Appointments.Commands.ScheduleAppointment;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleAppointment;

public class ScheduleAppointmentCommandValidatorTests
{
    private readonly ScheduleAppointmentCommandValidator _sut;

    public ScheduleAppointmentCommandValidatorTests()
    {
        _sut = new ScheduleAppointmentCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(1),
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(1),
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(1),
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DoctorId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            DateTime.UtcNow.Date.AddDays(1),
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentTypeId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenScheduledDateIsInThePast()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(-1),
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ScheduledDate);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartTimeIsAfterEndTime()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(1),
            new TimeSpan(11, 0, 0),
            new TimeSpan(10, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }
}
