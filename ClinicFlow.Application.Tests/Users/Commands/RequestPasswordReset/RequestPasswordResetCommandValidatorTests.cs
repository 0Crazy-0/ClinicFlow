using ClinicFlow.Application.Users.Commands.RequestPasswordReset;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.RequestPasswordReset;

public class RequestPasswordResetCommandValidatorTests
{
    private readonly RequestPasswordResetCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new RequestPasswordResetCommand("test@clinic.com");

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
        var command = new RequestPasswordResetCommand(email!);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}
