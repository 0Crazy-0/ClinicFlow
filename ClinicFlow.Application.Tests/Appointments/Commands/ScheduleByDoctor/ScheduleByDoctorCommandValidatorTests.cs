using ClinicFlow.Application.Appointments.Commands.ScheduleByDoctor;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByDoctor;

public class ScheduleByDoctorCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ScheduleByDoctorCommandValidator _sut;

    public ScheduleByDoctorCommandValidatorTests()
    {
        _sut = new ScheduleByDoctorCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleByDoctorCommand(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTargetPatientIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TargetPatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenScheduledDateIsInThePast()
    {
        // Arrange
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ScheduledDate)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBeInFuture);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartTimeIsAfterEndTime()
    {
        // Arrange
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(12, 0, 0),
            new TimeSpan(11, 0, 0),
            false,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
