using ClinicFlow.Application.Users.Commands.RegisterDoctorUser;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.RegisterDoctorUser;

public class RegisterDoctorUserCommandValidatorTests
{
    private readonly RegisterDoctorUserCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new RegisterDoctorUserCommand("doctor@clinic.com", "password123", "555-1234");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
