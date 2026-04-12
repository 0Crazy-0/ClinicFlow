using ClinicFlow.Application.Doctors.Queries.GetDoctorById;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Queries.GetDoctorById;

public class GetDoctorByIdQueryValidatorTests
{
    private readonly GetDoctorByIdQueryValidator _sut;

    public GetDoctorByIdQueryValidatorTests()
    {
        _sut = new GetDoctorByIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var query = new GetDoctorByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DoctorId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenDoctorIdIsValid()
    {
        // Arrange
        var query = new GetDoctorByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DoctorId);
    }
}
