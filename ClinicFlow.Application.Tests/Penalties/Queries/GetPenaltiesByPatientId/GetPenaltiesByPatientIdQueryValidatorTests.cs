using ClinicFlow.Application.Penalties.Queries.GetPenaltiesByPatientId;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Penalties.Queries.GetPenaltiesByPatientId;

public class GetPenaltiesByPatientIdQueryValidatorTests
{
    private readonly GetPenaltiesByPatientIdQueryValidator _sut;

    public GetPenaltiesByPatientIdQueryValidatorTests()
    {
        _sut = new GetPenaltiesByPatientIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenPatientIdIsValid()
    {
        // Arrange
        var query = new GetPenaltiesByPatientIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var query = new GetPenaltiesByPatientIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }
}
