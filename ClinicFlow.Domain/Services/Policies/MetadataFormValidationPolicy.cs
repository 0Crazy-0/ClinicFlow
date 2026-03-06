using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// A universal validation policy that ensures all required templates for an appointment type 
/// are present in the provided clinical details, and that their structure is valid.
/// </summary>
public class MetadataFormValidationPolicy(IJsonSchemaValidator jsonSchemaValidator) : IMedicalRecordValidationPolicy
{
    public void Validate(AppointmentTypeDefinition appointmentType, IEnumerable<IClinicalDetailRecord> providedDetails)
    {
        if (appointmentType is null) throw new DomainValidationException("Appointment type definition cannot be null.");
        if (providedDetails is null) throw new DomainValidationException("Provided details cannot be null.");

        var providedTemplateCodes = providedDetails.Select(d => d.TemplateCode).ToHashSet();

        foreach (var requiredTemplate in appointmentType.RequiredTemplates)
        {
            if (!providedTemplateCodes.Contains(requiredTemplate.Code))
            {
                throw new BusinessRuleValidationException(
                    $"Missing required clinical information. Template '{requiredTemplate.Code}' is required for appointment type '{appointmentType.Name}'.");
            }

            var providedDetail = providedDetails.First(d => d.TemplateCode == requiredTemplate.Code);
            ValidateJsonStructure(requiredTemplate, providedDetail);
        }
    }

    private void ValidateJsonStructure(ClinicalFormTemplate template, IClinicalDetailRecord? detail)
    {
        if (detail is null || string.IsNullOrWhiteSpace(detail.JsonDataPayload))
            throw new BusinessRuleValidationException($"No data payload provided for template '{template.Code}'.");

        if (string.IsNullOrWhiteSpace(template.JsonSchemaDefinition) || template.JsonSchemaDefinition is "{}") return; // No schema defined, nothing to validate

        if (!jsonSchemaValidator.ValidateSchema(template.JsonSchemaDefinition, detail.JsonDataPayload, out string? errorMessage))
            throw new BusinessRuleValidationException($"Validation failed for template '{template.Name}': {errorMessage}");

    }
}
