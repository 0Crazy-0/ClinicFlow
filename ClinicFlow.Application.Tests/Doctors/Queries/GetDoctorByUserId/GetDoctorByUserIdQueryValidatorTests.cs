using ClinicFlow.Application.Doctors.Queries.GetDoctorByUserId;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Queries.GetDoctorByUserId;

public class GetDoctorByUserIdQueryValidatorTests
{
    private readonly GetDoctorByUserIdQueryValidator _sut;

    public GetDoctorByUserIdQueryValidatorTests()
    {
        _sut = new GetDoctorByUserIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var query = new GetDoctorByUserIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenUserIdIsValid()
    {
        // Arrange
        var query = new GetDoctorByUserIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}
