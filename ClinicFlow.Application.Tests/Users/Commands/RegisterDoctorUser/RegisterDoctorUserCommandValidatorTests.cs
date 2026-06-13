using ClinicFlow.Application.Users.Commands.RegisterDoctorUser;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Commands.RegisterDoctorUser;

public class RegisterDoctorUserCommandValidatorTests
{
    private readonly RegisterDoctorUserCommandValidator _sut;

    public RegisterDoctorUserCommandValidatorTests()
    {
        _sut = new RegisterDoctorUserCommandValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new RegisterDoctorUserCommand("test@clinic.com", "password123", "555-1234");

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
        var command = new RegisterDoctorUserCommand(email!, "password123", "555-1234");

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
        var command = new RegisterDoctorUserCommand("not-an-email", "password123", "555-1234");

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
        var command = new RegisterDoctorUserCommand(email, "password123", "555-1234");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenPasswordIsEmpty(string? password)
    {
        // Arrange
        var command = new RegisterDoctorUserCommand("test@clinic.com", password!, "555-1234");

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
        var command = new RegisterDoctorUserCommand("test@clinic.com", "short", "555-1234");

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
        var command = new RegisterDoctorUserCommand("test@clinic.com", "password123", phone!);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldFail_WhenPhoneNumberIsTooShort()
    {
        // Arrange
        var command = new RegisterDoctorUserCommand(
            "test@clinic.com",
            "password123",
            new string('1', PhoneNumber.MinimumLength - 1)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldFail_WhenPhoneNumberIsTooLong()
    {
        // Arrange
        var phoneNumber = new string('1', PhoneNumber.MaximumLength + 1);
        var command = new RegisterDoctorUserCommand("test@clinic.com", "password123", phoneNumber);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
