using ClinicFlow.Application.Schedules.Commands.UpdateSchedule;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Schedules.Commands.UpdateSchedule;

public class UpdateScheduleCommandValidatorTests
{
    private readonly UpdateScheduleCommandValidator _sut;

    public UpdateScheduleCommandValidatorTests()
    {
        _sut = new UpdateScheduleCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new UpdateScheduleCommand(
            Guid.CreateVersion7(),
            DayOfWeek.Monday,
            new TimeOnly(10, 0),
            new TimeOnly(14, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new UpdateScheduleCommand(
            Guid.Empty,
            DayOfWeek.Monday,
            new TimeOnly(10, 0),
            new TimeOnly(14, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDayOfWeekIsInvalid()
    {
        // Arrange
        var command = new UpdateScheduleCommand(
            Guid.CreateVersion7(),
            (DayOfWeek)999,
            new TimeOnly(10, 0),
            new TimeOnly(14, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DayOfWeek)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEndTimeIsBeforeStartTime()
    {
        // Arrange
        var command = new UpdateScheduleCommand(
            Guid.CreateVersion7(),
            DayOfWeek.Monday,
            new TimeOnly(17, 0),
            new TimeOnly(9, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEndTimeEqualsStartTime()
    {
        // Arrange
        var command = new UpdateScheduleCommand(
            Guid.CreateVersion7(),
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(9, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
