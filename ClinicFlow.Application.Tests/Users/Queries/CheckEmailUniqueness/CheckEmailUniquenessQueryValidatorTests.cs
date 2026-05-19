using ClinicFlow.Application.Users.Queries.CheckEmailUniqueness;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Queries.CheckEmailUniqueness;

public class CheckEmailUniquenessQueryValidatorTests
{
    private readonly CheckEmailUniquenessQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenEmailIsValid()
    {
        // Arrange
        var query = new CheckEmailUniquenessQuery("user@clinic.com");

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenEmailIsEmpty(string? email)
    {
        // Arrange
        var query = new CheckEmailUniquenessQuery(email!);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var query = new CheckEmailUniquenessQuery("not-an-email");

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
