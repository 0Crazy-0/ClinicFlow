using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// A universal validation policy that ensures all required templates for an appointment type 
/// are present in the provided clinical details, and that their structure is valid.
/// </summary>
public class MetadataFormValidationPolicy(IJsonSchemaValidator jsonSchemaValidator) : IMedicalRecordValidationPolicy
{
    public void Validate(AppointmentTypeDefinition appointmentType, IEnumerable<IClinicalDetailRecord> providedDetails)
    {
        if (appointmentType is null) throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
        if (providedDetails is null) throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        var providedTemplateCodes = providedDetails.Select(d => d.TemplateCode).ToHashSet();

        foreach (var requiredTemplate in appointmentType.RequiredTemplates)
        {
            if (!providedTemplateCodes.Contains(requiredTemplate.Code))
            {
                throw new BusinessRuleValidationException(DomainErrors.MedicalEncounter.MissingRequiredTemplate);
            }

            var providedDetail = providedDetails.First(d => d.TemplateCode == requiredTemplate.Code);
            ValidateJsonStructure(requiredTemplate, providedDetail);
        }
    }

    private void ValidateJsonStructure(ClinicalFormTemplate template, IClinicalDetailRecord? detail)
    {
        if (detail is null || string.IsNullOrWhiteSpace(detail.JsonDataPayload))
            throw new BusinessRuleValidationException(DomainErrors.MedicalEncounter.MissingPayload);

        if (string.IsNullOrWhiteSpace(template.JsonSchemaDefinition) || template.JsonSchemaDefinition is "{}") return; // No schema defined, nothing to validate

        if (!jsonSchemaValidator.ValidateSchema(template.JsonSchemaDefinition, detail.JsonDataPayload, out string? errorMessage))
            throw new BusinessRuleValidationException($"{DomainErrors.MedicalEncounter.ValidationFailed}: {errorMessage}");

    }
}
