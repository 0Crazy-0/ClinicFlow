using ClinicFlow.Application.Patients.Queries.GetPatientById;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryValidatorTests
{
    private readonly GetPatientByIdQueryValidator _sut;

    public GetPatientByIdQueryValidatorTests()
    {
        _sut = new GetPatientByIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var query = new GetPatientByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenPatientIdIsValid()
    {
        // Arrange
        var query = new GetPatientByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PatientId);
    }
}
