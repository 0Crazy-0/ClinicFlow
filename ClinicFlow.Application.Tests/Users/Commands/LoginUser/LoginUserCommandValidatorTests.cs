using ClinicFlow.Application.Users.Commands.LoginUser;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.LoginUser;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new LoginUserCommand("test@clinic.com", "password123");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenEmailIsEmpty(string? email)
    {
        // Arrange
        var command = new LoginUserCommand(email!, "password123");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldFail_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var command = new LoginUserCommand("not-an-email", "password123");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenPasswordIsEmpty(string? password)
    {
        // Arrange
        var command = new LoginUserCommand("test@clinic.com", password!);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}
