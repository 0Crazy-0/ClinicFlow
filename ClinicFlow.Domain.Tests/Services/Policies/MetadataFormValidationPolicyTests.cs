using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Domain.Tests.Services.Policies;

public class MetadataFormValidationPolicyTests
{
    private readonly Mock<IJsonSchemaValidator> _mockSchemaValidator;
    private readonly MetadataFormValidationPolicy _sut;

    public MetadataFormValidationPolicyTests()
    {
        _mockSchemaValidator = new Mock<IJsonSchemaValidator>();
        _sut = new MetadataFormValidationPolicy(_mockSchemaValidator.Object);
    }

    [Fact]
    public void Validate_ShouldThrowArgumentNullException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () => _sut.Validate(null!, []);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_ShouldThrowArgumentNullException_WhenProvidedDetailsIsNull()
    {
        // Arrange & Act
        var act = () => _sut.Validate(CreateAppointmentTypeWithTemplates(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("providedDetails");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenNoRequiredTemplatesExist()
    {
        // Arrange & Act
        var act = () => _sut.Validate(CreateAppointmentTypeWithTemplates(), []);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ShouldThrowBusinessRuleValidationException_WhenRequiredTemplateIsMissing()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", "{}");
        var appointmentType = CreateAppointmentTypeWithTemplates(template);

        // Act
        var act = () => _sut.Validate(appointmentType, []);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingRequiredTemplate);
    }

    [Fact]
    public void Validate_ShouldThrowDomainValidationException_WhenJsonDataPayloadIsEmpty()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", "{}");
        var appointmentType = CreateAppointmentTypeWithTemplates(template);

        // Act
        var act = () =>
        {
            var detail = DynamicClinicalDetail.Create("VITALS", "   ");
            _sut.Validate(appointmentType, [detail]);
        };

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldSkipSchemaValidation_WhenSchemaDefinitionIsEmpty()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", "");
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var details = new List<DynamicClinicalDetail>
        {
            DynamicClinicalDetail.Create("VITALS", """{"bp":"120/80"}"""),
        };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should().NotThrow();
        _mockSchemaValidator.Verify(
            v => v.ValidateSchema(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public void Validate_ShouldSkipSchemaValidation_WhenSchemaDefinitionIsWhitespace()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create("VITALS", "Vitals", "Description", "   ");
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var detail = DynamicClinicalDetail.Create("VITALS", """{"bp":"120/80"}""");
        var details = new List<DynamicClinicalDetail> { detail };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should().NotThrow();
        _mockSchemaValidator.Verify(
            v => v.ValidateSchema(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenSchemaValidationPasses()
    {
        // Arrange
        var schemaDefinition = """{"type":"object","properties":{"bp":{"type":"string"}}}""";
        var template = ClinicalFormTemplate.Create(
            "VITALS",
            "Vitals",
            "Vital signs",
            schemaDefinition
        );
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var detail = DynamicClinicalDetail.Create("VITALS", """{"bp":"120/80"}""");
        var details = new List<DynamicClinicalDetail> { detail };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should().NotThrow();
        _mockSchemaValidator.Verify(
            v => v.ValidateSchema(schemaDefinition, detail.JsonDataPayload),
            Times.Once
        );
    }

    [Fact]
    public void Validate_ShouldValidateAllRequiredTemplates_WhenMultipleTemplatesExist()
    {
        // Arrange
        var schema1 = """{"type":"object"}""";
        var schema2 = """{"type":"object"}""";
        var template1 = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", schema1);
        var template2 = ClinicalFormTemplate.Create(
            "ALLERGIES",
            "Allergies",
            "Known allergies",
            schema2
        );
        var appointmentType = CreateAppointmentTypeWithTemplates(template1, template2);

        var payload1 = """{"bp":"120/80"}""";
        var payload2 = """{"allergy":"none"}""";
        var detail1 = DynamicClinicalDetail.Create("VITALS", payload1);
        var detail2 = DynamicClinicalDetail.Create("ALLERGIES", payload2);
        var details = new List<DynamicClinicalDetail> { detail1, detail2 };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should().NotThrow();
        _mockSchemaValidator.Verify(v => v.ValidateSchema(schema1, payload1), Times.Once);
        _mockSchemaValidator.Verify(v => v.ValidateSchema(schema2, payload2), Times.Once);
    }

    [Fact]
    public void Validate_ShouldThrowOnFirstMissingTemplate_WhenMultipleTemplatesRequired()
    {
        // Arrange
        var template1 = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", "{}");
        var template2 = ClinicalFormTemplate.Create(
            "ALLERGIES",
            "Allergies",
            "Known allergies",
            "{}"
        );
        var appointmentType = CreateAppointmentTypeWithTemplates(template1, template2);

        var detail = DynamicClinicalDetail.Create("ALLERGIES", """{"allergy":"none"}""");
        var details = new List<DynamicClinicalDetail> { detail };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingRequiredTemplate);
    }

    private static AppointmentTypeDefinition CreateAppointmentTypeWithTemplates(
        params ClinicalFormTemplate[] templates
    )
    {
        var appointmentType = AppointmentTypeDefinition.Create(
            Enums.AppointmentCategory.Other,
            Enums.AppointmentPurpose.Checkup,
            "General Checkup",
            "Standard checkup",
            EncounterDuration.FromMinutes(30)
        );

        foreach (var template in templates)
            appointmentType.AddRequiredTemplate(template);

        return appointmentType;
    }
}
