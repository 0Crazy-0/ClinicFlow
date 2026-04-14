using ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Schedules.Commands.SetupWeeklySchedule;

public class SetupWeeklyScheduleCommandValidatorTests
{
    private readonly SetupWeeklyScheduleCommandValidator _sut;

    public SetupWeeklyScheduleCommandValidatorTests()
    {
        _sut = new SetupWeeklyScheduleCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new SetupWeeklyScheduleCommand(
            Guid.NewGuid(),
            [
                new ScheduleSlot(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(13)),
                new ScheduleSlot(
                    DayOfWeek.Wednesday,
                    TimeSpan.FromHours(14),
                    TimeSpan.FromHours(18)
                ),
            ]
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
        var command = new SetupWeeklyScheduleCommand(
            Guid.Empty,
            [new ScheduleSlot(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(13))]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DoctorId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSlotsIsEmpty()
    {
        // Arrange
        var command = new SetupWeeklyScheduleCommand(Guid.NewGuid(), []);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slots);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSlotHasInvalidDayOfWeek()
    {
        // Arrange
        var command = new SetupWeeklyScheduleCommand(
            Guid.NewGuid(),
            [new ScheduleSlot((DayOfWeek)99, TimeSpan.FromHours(8), TimeSpan.FromHours(13))]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSlotEndTimeIsBeforeStartTime()
    {
        // Arrange
        var command = new SetupWeeklyScheduleCommand(
            Guid.NewGuid(),
            [new ScheduleSlot(DayOfWeek.Monday, TimeSpan.FromHours(17), TimeSpan.FromHours(8))]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
