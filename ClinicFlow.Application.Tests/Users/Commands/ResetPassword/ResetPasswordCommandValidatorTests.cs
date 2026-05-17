using ClinicFlow.Application.Users.Commands.ResetPassword;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.ResetPassword;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new ResetPasswordCommand("valid-token", "newpassword123");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenTokenIsEmpty(string? token)
    {
        // Arrange
        var command = new ResetPasswordCommand(token!, "newpassword123");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNewPasswordIsEmpty(string? password)
    {
        // Arrange
        var command = new ResetPasswordCommand("valid-token", password!);

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
        var command = new ResetPasswordCommand("valid-token", "short");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }
}
