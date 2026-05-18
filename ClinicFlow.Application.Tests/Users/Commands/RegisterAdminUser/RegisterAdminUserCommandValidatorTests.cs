using ClinicFlow.Application.Users.Commands.RegisterAdminUser;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.RegisterAdminUser;

public class RegisterAdminUserCommandValidatorTests
{
    private readonly RegisterAdminUserCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new RegisterAdminUserCommand("admin@clinic.com", "password123", "555-1234");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
