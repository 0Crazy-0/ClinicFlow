using ClinicFlow.Application.Users.Commands.RegisterUser;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new RegisterUserCommand("test@clinic.com", "password123", "555-1234");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
