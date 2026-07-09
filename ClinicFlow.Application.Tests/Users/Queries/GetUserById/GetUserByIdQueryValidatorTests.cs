using ClinicFlow.Application.Users.Queries.GetUserById;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Users.Queries.GetUserById;

public class GetUserByIdQueryValidatorTests
{
    private readonly GetUserByIdQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenUserIdIsValid()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
