using ClinicFlow.Application.Appointments.Commands.ScheduleAppointment;
using FluentAssertions;

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
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.PatientId));
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.DoctorId));
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, DateTime.UtcNow.Date.AddDays(1), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.AppointmentTypeId));
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenScheduledDateIsInThePast()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(-1), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.ScheduledDate));
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartTimeIsAfterEndTime()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1), new TimeSpan(11, 0, 0), new TimeSpan(10, 0, 0));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.StartTime));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.EndTime));
    }
}
