using ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateByCode;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Queries.GetClinicalFormTemplateByCode;

public class GetClinicalFormTemplateByCodeQueryValidatorTests
{
    private readonly GetClinicalFormTemplateByCodeQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCodeIsValid()
    {
        // Arrange
        var query = new GetClinicalFormTemplateByCodeQuery("INTAKE_V1");

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenCodeIsEmpty(string? code)
    {
        // Arrange
        var query = new GetClinicalFormTemplateByCodeQuery(code!);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}
