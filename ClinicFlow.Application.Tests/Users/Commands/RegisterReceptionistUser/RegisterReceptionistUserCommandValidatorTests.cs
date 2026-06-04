using ClinicFlow.Application.Users.Commands.RegisterReceptionistUser;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.RegisterReceptionistUser;

public class RegisterReceptionistUserCommandValidatorTests
{
    private readonly RegisterReceptionistUserCommandValidator _sut;

    public RegisterReceptionistUserCommandValidatorTests()
    {
        _sut = new RegisterReceptionistUserCommandValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new RegisterReceptionistUserCommand(
            "test@clinic.com",
            "password123",
            "555-1234"
        );

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
        var command = new RegisterReceptionistUserCommand(email!, "password123", "555-1234");

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
        var command = new RegisterReceptionistUserCommand(
            "not-an-email",
            "password123",
            "555-1234"
        );

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
        var command = new RegisterReceptionistUserCommand("test@clinic.com", password!, "555-1234");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldFail_WhenPasswordIsTooShort()
    {
        // Arrange
        var command = new RegisterReceptionistUserCommand("test@clinic.com", "short", "555-1234");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenPhoneNumberIsEmpty(string? phone)
    {
        // Arrange
        var command = new RegisterReceptionistUserCommand("test@clinic.com", "password123", phone!);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}
