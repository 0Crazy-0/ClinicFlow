using ClinicFlow.Application.Users.Commands.LogoutUser;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.LogoutUser;

public class LogoutUserCommandValidatorTests
{
    private readonly LogoutUserCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenUserIdIsValid()
    {
        // Arrange
        var command = new LogoutUserCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new LogoutUserCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}
