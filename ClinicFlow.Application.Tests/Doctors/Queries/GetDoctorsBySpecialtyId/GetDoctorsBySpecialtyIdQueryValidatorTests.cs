using ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Queries.GetDoctorsBySpecialtyId;

public class GetDoctorsBySpecialtyIdQueryValidatorTests
{
    private readonly GetDoctorsBySpecialtyIdQueryValidator _sut;

    public GetDoctorsBySpecialtyIdQueryValidatorTests()
    {
        _sut = new GetDoctorsBySpecialtyIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSpecialtyIdIsEmpty()
    {
        // Arrange
        var query = new GetDoctorsBySpecialtyIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SpecialtyId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenSpecialtyIdIsValid()
    {
        // Arrange
        var query = new GetDoctorsBySpecialtyIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SpecialtyId);
    }
}
