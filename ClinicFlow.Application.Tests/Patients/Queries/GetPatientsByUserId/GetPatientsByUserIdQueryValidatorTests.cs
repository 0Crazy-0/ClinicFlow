using ClinicFlow.Application.Patients.Queries.GetPatientsByUserId;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Queries.GetPatientsByUserId;

public class GetPatientsByUserIdQueryValidatorTests
{
    private readonly GetPatientsByUserIdQueryValidator _sut;

    public GetPatientsByUserIdQueryValidatorTests()
    {
        _sut = new GetPatientsByUserIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var query = new GetPatientsByUserIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenUserIdIsValid()
    {
        // Arrange
        var query = new GetPatientsByUserIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}
