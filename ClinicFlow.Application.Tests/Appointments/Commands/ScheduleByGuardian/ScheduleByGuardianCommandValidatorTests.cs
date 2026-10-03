using ClinicFlow.Application.Appointments.Commands.ScheduleByGuardian;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByGuardian;

public class ScheduleByGuardianCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ScheduleByGuardianCommandValidator _sut;

    public ScheduleByGuardianCommandValidatorTests()
    {
        _sut = new ScheduleByGuardianCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new ScheduleByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10),
            new TimeOnly(11),
            "Notes"
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
        var command = new ScheduleByGuardianCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10),
            new TimeOnly(11)
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
        var command = new ScheduleByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10),
            new TimeOnly(11)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TargetPatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.Empty,
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10),
            new TimeOnly(11)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new ScheduleByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.Empty,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10),
            new TimeOnly(11)
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
        var command = new ScheduleByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(-1)),
            new TimeOnly(10),
            new TimeOnly(11)
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
        var command = new ScheduleByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(12),
            new TimeOnly(11)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenGuardianNotesAreTooLong()
    {
        // Arrange
        var command = new ScheduleByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10),
            new TimeOnly(11),
            new string('a', 501)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.GuardianNotes)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
