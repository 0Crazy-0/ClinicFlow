using ClinicFlow.Application.Schedules.Commands.DeactivateSchedule;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Schedules.Commands.DeactivateSchedule;

public class DeactivateScheduleCommandValidatorTests
{
    private readonly DeactivateScheduleCommandValidator _sut;

    public DeactivateScheduleCommandValidatorTests()
    {
        _sut = new DeactivateScheduleCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenScheduleIdIsValid()
    {
        // Arrange
        var command = new DeactivateScheduleCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenScheduleIdIsEmpty()
    {
        // Arrange
        var command = new DeactivateScheduleCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ScheduleId);
    }
}
