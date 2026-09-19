using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// A universal validation policy that ensures all required templates for an appointment type
/// are present in the provided clinical details, and that their structure is valid.
/// </summary>
public class MetadataFormValidationPolicy(IJsonSchemaValidator jsonSchemaValidator)
{
    public void Validate(
        AppointmentTypeDefinition appointmentType,
        IEnumerable<DynamicClinicalDetail> providedDetails
    )
    {
        ArgumentNullException.ThrowIfNull(appointmentType);
        ArgumentNullException.ThrowIfNull(providedDetails);

        foreach (var requiredTemplate in appointmentType.RequiredTemplates)
        {
            var providedDetail =
                providedDetails.FirstOrDefault(d => d.TemplateCode == requiredTemplate.Code)
                ?? throw new BusinessRuleValidationException(
                    DomainErrors.MedicalEncounter.MissingRequiredTemplate
                );

            // No schema defined, nothing to validate
            if (requiredTemplate.JsonSchemaDefinition is not "{}")
                jsonSchemaValidator.ValidateSchema(
                    requiredTemplate.JsonSchemaDefinition,
                    providedDetail.JsonDataPayload
                );
        }
    }
}
