using ClinicFlow.Application.Users.Commands.RequestPasswordReset;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
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

    [Fact]
    public void Validate_ShouldFail_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var command = new RequestPasswordResetCommand("not-an-email");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldFail_WhenEmailIsTooLong()
    {
        // Arrange
        var domain = "@example.com";
        var email = new string('a', EmailAddress.MaximumLength - domain.Length + 1) + domain;
        var command = new RequestPasswordResetCommand(email);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
