using ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByPatient;

public class RescheduleByPatientCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly RescheduleByPatientCommandValidator _sut;

    public RescheduleByPatientCommandValidatorTests()
    {
        _sut = new RescheduleByPatientCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
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
        var command = new RescheduleByPatientCommand(
            Guid.Empty,
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.Empty,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNewDateIsInThePast()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(-1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewDate)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBeInFuture);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNewStartTimeIsAfterNewEndTime()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(12, 0),
            new TimeOnly(11, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewEndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNewPatientNotesAreTooLong()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            new string('a', 501)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewPatientNotes)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
