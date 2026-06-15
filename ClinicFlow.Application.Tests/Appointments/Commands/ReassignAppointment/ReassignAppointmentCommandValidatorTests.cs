using ClinicFlow.Application.Appointments.Commands.ReassignAppointment;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ReassignAppointment;

public class ReassignAppointmentCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ReassignAppointmentCommandValidator _sut;

    public ReassignAppointmentCommandValidatorTests()
    {
        _sut = new ReassignAppointmentCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new ReassignAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new ReassignAppointmentCommand(
            Guid.Empty,
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldFail_WhenNewDoctorIdIsEmpty()
    {
        // Arrange
        var command = new ReassignAppointmentCommand(
            Guid.NewGuid(),
            Guid.Empty,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewDoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldFail_WhenNewDateIsInThePast()
    {
        // Arrange
        var command = new ReassignAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewDate)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBeInFuture);
    }

    [Fact]
    public void Validate_ShouldFail_WhenNewEndTimeIsBeforeNewStartTime()
    {
        // Arrange
        var command = new ReassignAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeOnly(10, 0),
            new TimeOnly(9, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewEndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
