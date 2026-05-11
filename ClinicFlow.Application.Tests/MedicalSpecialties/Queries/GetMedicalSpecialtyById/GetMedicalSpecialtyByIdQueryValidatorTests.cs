using ClinicFlow.Application.MedicalSpecialties.Queries.GetMedicalSpecialtyById;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Queries.GetMedicalSpecialtyById;

public class GetMedicalSpecialtyByIdQueryValidatorTests
{
    private readonly GetMedicalSpecialtyByIdQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenIdIsValid()
    {
        // Arrange
        var query = new GetMedicalSpecialtyByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        // Arrange
        var query = new GetMedicalSpecialtyByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MedicalSpecialtyId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
