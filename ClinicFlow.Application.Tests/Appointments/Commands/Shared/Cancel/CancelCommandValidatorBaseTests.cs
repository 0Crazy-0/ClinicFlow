using ClinicFlow.Application.Appointments.Commands.Shared.Cancel;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.Shared.Cancel;

public record DummyCancelCommand(Guid AppointmentId, Guid InitiatorUserId, string? Reason)
    : ICancelCommand;

public class DummyCancelCommandValidator : CancelCommandValidatorBase<DummyCancelCommand> { }

public class CancelCommandValidatorBaseTests
{
    private readonly DummyCancelCommandValidator _sut;

    public CancelCommandValidatorBaseTests()
    {
        _sut = new DummyCancelCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new DummyCancelCommand(Guid.NewGuid(), Guid.NewGuid(), "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new DummyCancelCommand(Guid.Empty, Guid.NewGuid(), "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new DummyCancelCommand(Guid.NewGuid(), Guid.Empty, "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitiatorUserId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReasonExceedsMaximumLength()
    {
        // Arrange
        var longReason = new string('a', 501);
        var command = new DummyCancelCommand(Guid.NewGuid(), Guid.NewGuid(), longReason);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
