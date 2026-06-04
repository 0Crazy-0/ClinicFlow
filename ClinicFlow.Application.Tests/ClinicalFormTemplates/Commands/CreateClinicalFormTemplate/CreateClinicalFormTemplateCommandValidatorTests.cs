using ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation.TestHelper;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

public class CreateClinicalFormTemplateCommandValidatorTests
{
    private readonly Mock<IJsonSchemaDefinitionValidator> _schemaValidatorMock;
    private readonly CreateClinicalFormTemplateCommandValidator _sut;

    public CreateClinicalFormTemplateCommandValidatorTests()
    {
        _schemaValidatorMock = new Mock<IJsonSchemaDefinitionValidator>();
        _schemaValidatorMock
            .Setup(x => x.IsValidSchema(It.IsAny<string>(), out It.Ref<string?>.IsAny))
            .Returns(true);

        _sut = new CreateClinicalFormTemplateCommandValidator(_schemaValidatorMock.Object);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreateClinicalFormTemplateCommand(
            "BP_FORM",
            "Blood Pressure",
            "Form description",
            "{}"
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
        var command = new CreateClinicalFormTemplateCommand(
            code!,
            "Blood Pressure",
            "Form description",
            "{}"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var command = new CreateClinicalFormTemplateCommand("BP_FORM", name!, "Description", "{}");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNameExceedsMaximumLength()
    {
        // Arrange
        var tooLongName = new string('A', 101);
        var command = new CreateClinicalFormTemplateCommand(
            "BP_FORM",
            tooLongName,
            "Description",
            "{}"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDescriptionExceedsMaximumLength()
    {
        // Arrange
        var tooLongDescription = new string('B', 501);
        var command = new CreateClinicalFormTemplateCommand(
            "BP_FORM",
            "Blood Pressure",
            tooLongDescription,
            "{}"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSchemaIsInvalid()
    {
        // Arrange
        string? errorMsg = "Invalid structure";
        _schemaValidatorMock
            .Setup(x => x.IsValidSchema(It.IsAny<string>(), out errorMsg))
            .Returns(false);

        var command = new CreateClinicalFormTemplateCommand(
            "BP_FORM",
            "Blood Pressure",
            "Desc",
            "{invalid}"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.JsonSchemaDefinition)
            .WithErrorMessage(DomainErrors.Validation.InvalidFormat);
    }
}
