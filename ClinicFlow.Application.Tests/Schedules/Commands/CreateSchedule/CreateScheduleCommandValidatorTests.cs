using ClinicFlow.Application.Schedules.Commands.CreateSchedule;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Schedules.Commands.CreateSchedule;

public class CreateScheduleCommandValidatorTests
{
    private readonly CreateScheduleCommandValidator _sut;

    public CreateScheduleCommandValidatorTests()
    {
        _sut = new CreateScheduleCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new CreateScheduleCommand(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(17)
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
        var command = new CreateScheduleCommand(
            Guid.Empty,
            DayOfWeek.Monday,
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(17)
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
        var command = new CreateScheduleCommand(
            Guid.NewGuid(),
            (DayOfWeek)99,
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(17)
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
        var command = new CreateScheduleCommand(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            TimeSpan.FromHours(17),
            TimeSpan.FromHours(9)
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
        var command = new CreateScheduleCommand(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(9)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
