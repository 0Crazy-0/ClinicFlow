using ClinicFlow.Application.Users.Commands.SendPhoneVerification;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.SendPhoneVerification;

public class SendPhoneVerificationCommandValidatorTests
{
    private readonly SendPhoneVerificationCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenUserIdIsProvided()
    {
        // Arrange
        var command = new SendPhoneVerificationCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new SendPhoneVerificationCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
