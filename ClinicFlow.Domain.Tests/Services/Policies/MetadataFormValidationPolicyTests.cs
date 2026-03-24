using System.Reflection;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Policies;
using FluentAssertions;
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
    public void Validate_ShouldThrowDomainValidationException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () => _sut.Validate(null!, []);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Validate_ShouldThrowDomainValidationException_WhenProvidedDetailsIsNull()
    {
        // Arrange & Act
        var act = () => _sut.Validate(CreateAppointmentTypeWithTemplates(), null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
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
    public void Validate_ShouldThrowBusinessRuleValidationException_WhenDetailIsNotDynamicClinicalDetail()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", "{}");
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var nonDynamicDetail = new StubClinicalDetailRecord("VITALS");
        var details = new List<IClinicalDetailRecord> { nonDynamicDetail };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingPayload);
    }

    [Fact]
    public void Validate_ShouldThrowBusinessRuleValidationException_WhenJsonDataPayloadIsEmpty()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", "{}");
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var detail = DynamicClinicalDetail.Create("VITALS", "   ");
        var details = new List<IClinicalDetailRecord> { detail };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingPayload);
    }

    [Fact]
    public void Validate_ShouldSkipSchemaValidation_WhenSchemaDefinitionIsEmpty()
    {
        // Arrange
        var template = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", "");
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var detail = DynamicClinicalDetail.Create("VITALS", "{\"bp\":\"120/80\"}");
        var details = new List<IClinicalDetailRecord> { detail };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should().NotThrow();
        _mockSchemaValidator.Verify(
            v =>
                v.ValidateSchema(It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string?>.IsAny),
            Times.Never
        );
    }

    [Fact]
    public void Validate_ShouldSkipSchemaValidation_WhenSchemaDefinitionIsWhitespace()
    {
        // Arrange
        var template = CreateTemplateWithSchema("VITALS", "Vitals", "   ");
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var detail = DynamicClinicalDetail.Create("VITALS", "{\"bp\":\"120/80\"}");
        var details = new List<IClinicalDetailRecord> { detail };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should().NotThrow();
        _mockSchemaValidator.Verify(
            v =>
                v.ValidateSchema(It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string?>.IsAny),
            Times.Never
        );
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenSchemaValidationPasses()
    {
        // Arrange
        var schemaDefinition =
            "{\"type\":\"object\",\"properties\":{\"bp\":{\"type\":\"string\"}}}";
        var template = ClinicalFormTemplate.Create(
            "VITALS",
            "Vitals",
            "Vital signs",
            schemaDefinition
        );
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var payload = "{\"bp\":\"120/80\"}";
        var detail = DynamicClinicalDetail.Create("VITALS", payload);
        var details = new List<IClinicalDetailRecord> { detail };

        string? errorMsg = null;
        _mockSchemaValidator
            .Setup(v => v.ValidateSchema(schemaDefinition, payload, out errorMsg))
            .Returns(true);

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should().NotThrow();
        _mockSchemaValidator.Verify(
            v => v.ValidateSchema(schemaDefinition, payload, out It.Ref<string?>.IsAny),
            Times.Once
        );
    }

    [Fact]
    public void Validate_ShouldThrowBusinessRuleValidationException_WhenSchemaValidationFails()
    {
        // Arrange
        var schemaDefinition = "{\"type\":\"object\",\"required\":[\"bp\"]}";
        var template = ClinicalFormTemplate.Create(
            "VITALS",
            "Vitals",
            "Vital signs",
            schemaDefinition
        );
        var appointmentType = CreateAppointmentTypeWithTemplates(template);
        var payload = "{\"temperature\":\"37\"}";
        var detail = DynamicClinicalDetail.Create("VITALS", payload);
        var details = new List<IClinicalDetailRecord> { detail };

        string? errorMsg = "Required property 'bp' is missing.";
        _mockSchemaValidator
            .Setup(v => v.ValidateSchema(schemaDefinition, payload, out errorMsg))
            .Returns(false);

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage($"{DomainErrors.MedicalEncounter.ValidationFailed}: {errorMsg}");
    }

    [Fact]
    public void Validate_ShouldValidateAllRequiredTemplates_WhenMultipleTemplatesExist()
    {
        // Arrange
        var schema1 = "{\"type\":\"object\"}";
        var schema2 = "{\"type\":\"object\"}";
        var template1 = ClinicalFormTemplate.Create("VITALS", "Vitals", "Vital signs", schema1);
        var template2 = ClinicalFormTemplate.Create(
            "ALLERGIES",
            "Allergies",
            "Known allergies",
            schema2
        );
        var appointmentType = CreateAppointmentTypeWithTemplates(template1, template2);

        var payload1 = "{\"bp\":\"120/80\"}";
        var payload2 = "{\"allergy\":\"none\"}";
        var detail1 = DynamicClinicalDetail.Create("VITALS", payload1);
        var detail2 = DynamicClinicalDetail.Create("ALLERGIES", payload2);
        var details = new List<IClinicalDetailRecord> { detail1, detail2 };

        string? errorMsg = null;
        _mockSchemaValidator
            .Setup(v => v.ValidateSchema(It.IsAny<string>(), It.IsAny<string>(), out errorMsg))
            .Returns(true);

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should().NotThrow();
        _mockSchemaValidator.Verify(
            v => v.ValidateSchema(schema1, payload1, out It.Ref<string?>.IsAny),
            Times.Once
        );
        _mockSchemaValidator.Verify(
            v => v.ValidateSchema(schema2, payload2, out It.Ref<string?>.IsAny),
            Times.Once
        );
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

        var detail = DynamicClinicalDetail.Create("ALLERGIES", "{\"allergy\":\"none\"}");
        var details = new List<IClinicalDetailRecord> { detail };

        // Act
        var act = () => _sut.Validate(appointmentType, details);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.MissingRequiredTemplate);
    }

    private class StubClinicalDetailRecord(string templateCode) : IClinicalDetailRecord
    {
        public string TemplateCode => templateCode;
        public string JsonDataPayload => string.Empty;
    }

    private static AppointmentTypeDefinition CreateAppointmentTypeWithTemplates(
        params ClinicalFormTemplate[] templates
    )
    {
        var appointmentType = AppointmentTypeDefinition.Create(
            Enums.AppointmentCategory.Checkup,
            "General Checkup",
            "Standard checkup",
            TimeSpan.FromMinutes(30)
        );

        foreach (var template in templates)
            appointmentType.AddRequiredTemplate(template);

        return appointmentType;
    }

    private static ClinicalFormTemplate CreateTemplateWithSchema(
        string code,
        string name,
        string schemaDefinition
    )
    {
        var template = (ClinicalFormTemplate)
            Activator.CreateInstance(typeof(ClinicalFormTemplate), true)!;
        SetPrivateProperty(template, nameof(ClinicalFormTemplate.Code), code);
        SetPrivateProperty(template, nameof(ClinicalFormTemplate.Name), name);
        SetPrivateProperty(
            template,
            nameof(ClinicalFormTemplate.JsonSchemaDefinition),
            schemaDefinition
        );
        return template;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();

        while (type != null)
        {
            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly
            );

            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }

            type = type.BaseType;
        }
    }
}
