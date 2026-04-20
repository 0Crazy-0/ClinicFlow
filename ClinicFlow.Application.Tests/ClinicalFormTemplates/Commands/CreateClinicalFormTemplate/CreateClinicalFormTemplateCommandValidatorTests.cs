using ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation.TestHelper;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

public class CreateClinicalFormTemplateCommandValidatorTests
{
    private readonly CreateClinicalFormTemplateCommandValidator _sut;

    public CreateClinicalFormTemplateCommandValidatorTests()
    {
        var schemaValidatorMock = new Mock<IJsonSchemaDefinitionValidator>();
        schemaValidatorMock
            .Setup(x => x.IsValidSchema(It.IsAny<string>(), out It.Ref<string?>.IsAny))
            .Returns(true);

        _sut = new CreateClinicalFormTemplateCommandValidator(schemaValidatorMock.Object);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new CreateClinicalFormTemplateCommand(
            "CARDIO_01",
            "Cardiology Form",
            "For cardiac evaluations",
            """{"fields":[]}"""
        );

        // Act
        var result = _sut.TestValidate(command);

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
        var command = new CreateClinicalFormTemplateCommand(code!, "Name", "Desc", "{}");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
}
