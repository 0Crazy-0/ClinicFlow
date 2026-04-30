using ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Commands.Shared.Reschedule;

public record DummyRescheduleCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateTime NewDate,
    TimeSpan NewStartTime,
    TimeSpan NewEndTime
) : IRescheduleCommand;

public class DummyRescheduleCommandValidator(TimeProvider timeProvider)
    : RescheduleCommandValidatorBase<DummyRescheduleCommand>(timeProvider);

public class RescheduleCommandValidatorBaseTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly DummyRescheduleCommandValidator _sut;

    public RescheduleCommandValidatorBaseTests()
    {
        _sut = new DummyRescheduleCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new DummyRescheduleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new DummyRescheduleCommand(
            Guid.NewGuid(),
            Guid.Empty,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new DummyRescheduleCommand(
            Guid.Empty,
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitiatorUserId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNewDateIsInThePast()
    {
        // Arrange
        var command = new DummyRescheduleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
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
        var command = new DummyRescheduleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(12, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewStartTime)
            .WithErrorMessage(DomainErrors.Validation.StartTimeMustBeBeforeEndTime);
        result
            .ShouldHaveValidationErrorFor(x => x.NewEndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
