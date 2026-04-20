using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class ClinicalFormTemplateTests
{
    [Fact]
    public void Create_ShouldCreateTemplate_WhenAllFieldsAreValid()
    {
        // Act
        var template = ClinicalFormTemplate.Create(
            "CARDIO_01",
            "Cardiology Form",
            "For cardiac evaluations",
            """{"fields":["heartRate"]}"""
        );

        // Assert
        template.Code.Should().Be("CARDIO_01");
        template.Name.Should().Be("Cardiology Form");
        template.Description.Should().Be("For cardiac evaluations");
        template.JsonSchemaDefinition.Should().Be("""{"fields":["heartRate"]}""");
    }

    [Fact]
    public void Create_ShouldDefaultSchemaToEmptyObject_WhenJsonIsNullOrWhitespace()
    {
        // Act
        var template = ClinicalFormTemplate.Create("CODE_01", "Name", "Desc", "   ");

        // Assert
        template.JsonSchemaDefinition.Should().Be("{}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenCodeIsEmpty(string? code)
    {
        // Act
        var act = () => ClinicalFormTemplate.Create(code!, "Name", "Desc", "{}");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Act
        var act = () => ClinicalFormTemplate.Create("CODE_01", name!, "Desc", "{}");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndDescription_WhenValid()
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        template.UpdateDetails("Updated Name", "Updated Description");

        // Assert
        template.Name.Should().Be("Updated Name");
        template.Description.Should().Be("Updated Description");
    }

    [Fact]
    public void UpdateDetails_ShouldNotChangeCode_WhenUpdating()
    {
        // Arrange
        var template = CreateDefaultTemplate();
        var originalCode = template.Code;

        // Act
        template.UpdateDetails("New Name", "New Desc");

        // Assert
        template.Code.Should().Be(originalCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        var act = () => template.UpdateDetails(name!, "Some description");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void UpdateSchema_ShouldReplaceJsonSchema_WhenValueIsValid()
    {
        // Arrange
        var template = CreateDefaultTemplate();
        var newSchema = """{"fields":["temperature","bloodPressure"]}""";

        // Act
        template.UpdateSchema(newSchema);

        // Assert
        template.JsonSchemaDefinition.Should().Be(newSchema);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateSchema_ShouldDefaultToEmptyObject_WhenValueIsNullOrWhitespace(string? schema)
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        template.UpdateSchema(schema!);

        // Assert
        template.JsonSchemaDefinition.Should().Be("{}");
    }

    private static ClinicalFormTemplate CreateDefaultTemplate() =>
        ClinicalFormTemplate.Create(
            "TEMPLATE_001",
            "Blood Test Form",
            "Standard blood test",
            """{"fields":[]}"""
        );
}
