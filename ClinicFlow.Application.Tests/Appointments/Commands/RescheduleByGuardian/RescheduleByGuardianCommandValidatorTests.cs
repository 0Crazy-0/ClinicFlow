using ClinicFlow.Application.Appointments.Commands.RescheduleByGuardian;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByGuardian;

public class RescheduleByGuardianCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly RescheduleByGuardianCommandValidator _sut;

    public RescheduleByGuardianCommandValidatorTests()
    {
        _sut = new RescheduleByGuardianCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new RescheduleByGuardianCommand(
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
        var command = new RescheduleByGuardianCommand(
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
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new RescheduleByGuardianCommand(
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
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNewDateIsInThePast()
    {
        // Arrange
        var command = new RescheduleByGuardianCommand(
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
            .ShouldHaveValidationErrorFor(x => x.NewDate)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBeInFuture);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNewStartTimeIsAfterNewEndTime()
    {
        // Arrange
        var command = new RescheduleByGuardianCommand(
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
            .ShouldHaveValidationErrorFor(x => x.NewEndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNewGuardianNotesAreTooLong()
    {
        // Arrange
        var command = new RescheduleByGuardianCommand(
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
            .ShouldHaveValidationErrorFor(x => x.NewGuardianNotes)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
