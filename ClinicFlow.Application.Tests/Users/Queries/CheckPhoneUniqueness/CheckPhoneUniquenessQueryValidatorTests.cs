using ClinicFlow.Application.Users.Queries.CheckPhoneUniqueness;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Queries.CheckPhoneUniqueness;

public class CheckPhoneUniquenessQueryValidatorTests
{
    private readonly CheckPhoneUniquenessQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenPhoneNumberIsValid()
    {
        // Arrange
        var query = new CheckPhoneUniquenessQuery("555-1234");

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenPhoneNumberIsEmpty(string? phoneNumber)
    {
        // Arrange
        var query = new CheckPhoneUniquenessQuery(phoneNumber!);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}
