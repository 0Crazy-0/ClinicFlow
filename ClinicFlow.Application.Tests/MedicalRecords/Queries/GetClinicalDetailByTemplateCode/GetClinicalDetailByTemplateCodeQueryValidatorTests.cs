using ClinicFlow.Application.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;

public class GetClinicalDetailByTemplateCodeQueryValidatorTests
{
    private readonly GetClinicalDetailByTemplateCodeQueryValidator _sut;

    public GetClinicalDetailByTemplateCodeQueryValidatorTests()
    {
        _sut = new GetClinicalDetailByTemplateCodeQueryValidator();
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetClinicalDetailByTemplateCodeQuery(Guid.NewGuid(), "VITALS");

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMedicalRecordIdIsEmpty()
    {
        // Arrange
        var query = new GetClinicalDetailByTemplateCodeQuery(Guid.Empty, "VITALS");

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MedicalRecordId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenTemplateCodeIsEmpty(string? templateCode)
    {
        // Arrange
        var query = new GetClinicalDetailByTemplateCodeQuery(Guid.NewGuid(), templateCode!);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TemplateCode);
    }
}
