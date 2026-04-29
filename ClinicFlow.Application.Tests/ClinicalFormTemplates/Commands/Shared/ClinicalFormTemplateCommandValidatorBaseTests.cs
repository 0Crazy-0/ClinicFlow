using ClinicFlow.Application.ClinicalFormTemplates.Commands.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation.TestHelper;
using Moq;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.Shared;

public record DummyClinicalFormTemplateCommand(
    string Name,
    string Description,
    string JsonSchemaDefinition
) : IClinicalFormTemplateCommand;

public class DummyClinicalFormTemplateCommandValidator
    : ClinicalFormTemplateCommandValidatorBase<DummyClinicalFormTemplateCommand>
{
    public DummyClinicalFormTemplateCommandValidator(
        IJsonSchemaDefinitionValidator schemaDefinitionValidator
    )
        : base(schemaDefinitionValidator) { }
}

public class ClinicalFormTemplateCommandValidatorBaseTests
{
    private readonly Mock<IJsonSchemaDefinitionValidator> _schemaValidatorMock;
    private readonly DummyClinicalFormTemplateCommandValidator _sut;

    public ClinicalFormTemplateCommandValidatorBaseTests()
    {
        _schemaValidatorMock = new Mock<IJsonSchemaDefinitionValidator>();
        _schemaValidatorMock
            .Setup(x => x.IsValidSchema(It.IsAny<string>(), out It.Ref<string?>.IsAny))
            .Returns(true);

        _sut = new DummyClinicalFormTemplateCommandValidator(_schemaValidatorMock.Object);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new DummyClinicalFormTemplateCommand(
            "Blood Test Form",
            "Standard blood test",
            """{"fields":[]}"""
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenJsonSchemaIsEmpty()
    {
        // Arrange
        var command = new DummyClinicalFormTemplateCommand("Blood Test Form", "Description", "");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var command = new DummyClinicalFormTemplateCommand(name!, "Description", "{}");

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
        var command = new DummyClinicalFormTemplateCommand(tooLongName, "Description", "{}");

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
        var command = new DummyClinicalFormTemplateCommand("Valid Name", tooLongDescription, "{}");

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

        var sut = new DummyClinicalFormTemplateCommandValidator(_schemaValidatorMock.Object);
        var command = new DummyClinicalFormTemplateCommand("Name", "Desc", "{invalid}");

        // Act
        var result = sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.JsonSchemaDefinition)
            .WithErrorMessage(DomainErrors.Validation.InvalidFormat);
    }
}
