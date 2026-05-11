using ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateById;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Queries.GetClinicalFormTemplateById;

public class GetClinicalFormTemplateByIdQueryValidatorTests
{
    private readonly GetClinicalFormTemplateByIdQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenIdIsValid()
    {
        // Arrange
        var query = new GetClinicalFormTemplateByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        // Arrange
        var query = new GetClinicalFormTemplateByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ClinicalFormTemplateId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
