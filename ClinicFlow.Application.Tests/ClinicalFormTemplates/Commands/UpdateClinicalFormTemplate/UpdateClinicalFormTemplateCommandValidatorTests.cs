using ClinicFlow.Application.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation.TestHelper;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;

public class UpdateClinicalFormTemplateCommandValidatorTests
{
    private readonly UpdateClinicalFormTemplateCommandValidator _sut;

    public UpdateClinicalFormTemplateCommandValidatorTests()
    {
        var schemaValidatorMock = new Mock<IJsonSchemaDefinitionValidator>();
        schemaValidatorMock
            .Setup(x => x.IsValidSchema(It.IsAny<string>(), out It.Ref<string?>.IsAny))
            .Returns(true);

        _sut = new UpdateClinicalFormTemplateCommandValidator(schemaValidatorMock.Object);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new UpdateClinicalFormTemplateCommand(
            Guid.NewGuid(),
            "Cardiology Form",
            "Description",
            """{"fields":[]}"""
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTemplateIdIsEmpty()
    {
        // Arrange
        var command = new UpdateClinicalFormTemplateCommand(
            Guid.Empty,
            "Name",
            "Description",
            "{}"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TemplateId);
    }
}
