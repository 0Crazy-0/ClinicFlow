using ClinicFlow.Application.Users.Commands.ChangePassword;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.ChangePassword;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "oldpassword", "newpassword123");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.Empty, "oldpassword", "newpassword123");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenCurrentPasswordIsEmpty(string? currentPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), currentPassword!, "newpassword123");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNewPasswordIsEmpty(string? newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "oldpassword", newPassword!);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldFail_WhenNewPasswordIsTooShort()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "oldpassword", "short");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }
}
