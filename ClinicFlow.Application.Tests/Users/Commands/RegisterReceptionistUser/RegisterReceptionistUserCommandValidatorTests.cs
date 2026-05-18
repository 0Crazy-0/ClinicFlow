using ClinicFlow.Application.Users.Commands.RegisterReceptionistUser;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.RegisterReceptionistUser;

public class RegisterReceptionistUserCommandValidatorTests
{
    private readonly RegisterReceptionistUserCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new RegisterReceptionistUserCommand(
            "receptionist@clinic.com",
            "password123",
            "555-1234"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
