using ClinicFlow.Application.Users.Commands.VerifyPhone;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.VerifyPhone;

public class VerifyPhoneCommandValidatorTests
{
    private readonly VerifyPhoneCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvided()
    {
        // Arrange
        var command = new VerifyPhoneCommand(Guid.NewGuid(), "123456");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new VerifyPhoneCommand(Guid.Empty, "123456");

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
    public void Validate_ShouldHaveError_WhenCodeIsEmpty(string? invalidCode)
    {
        // Arrange
        var command = new VerifyPhoneCommand(Guid.NewGuid(), invalidCode!);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}
