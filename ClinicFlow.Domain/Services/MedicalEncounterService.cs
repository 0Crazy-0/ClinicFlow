using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service responsible for orchestrating the rules around a medical encounter.
/// </summary>
public class MedicalEncounterService(IEnumerable<IMedicalRecordValidationPolicy> policies, IJsonSchemaValidator jsonSchemaValidator)
{
    /// <summary>
    /// Validates business rules against the provided context and updates the medical record with the given details.
    /// </summary>
    /// <exception cref="BusinessRuleValidationException">Thrown if any domain rule is violated.</exception>
    public void ValidateAndCompleteRecord(MedicalRecord record, MedicalEncounterContext context)
    {
        if (record is null) throw new DomainValidationException("The medical record is required and cannot be null.");
        if (context is null) throw new DomainValidationException("The medical encounter context is required and cannot be null.");
        if (context.ExpectedDoctor is null) throw new BusinessRuleValidationException("Expected doctor context is missing.");
        if (context.Appointment is null) throw new BusinessRuleValidationException("Appointment context is missing.");
        if (context.AppointmentTypeDefinition is null) throw new BusinessRuleValidationException("Appointment type definition context is missing.");

        if (record.DoctorId != context.ExpectedDoctor.Id)
            throw new BusinessRuleValidationException("The doctor provided does not match the doctor assigned to the medical record.");

        if (record.AppointmentId != context.Appointment.Id)
            throw new BusinessRuleValidationException("The appointment provided does not match the appointment assigned to the medical record.");

        foreach (var policy in policies)
            policy.Validate(context.AppointmentTypeDefinition, context.ProvidedDetails);

        foreach (var detail in context.ProvidedDetails)
            record.AddClinicalDetail(detail);

    }

    /// <summary>
    /// Validates and appends a single clinical detail to an existing medical record.
    /// Enforces that the detail's JSON structure complies with the template schema if one exists.
    /// </summary>
    public void AppendClinicalDetail(MedicalRecord record, IClinicalDetailRecord newDetail, ClinicalFormTemplate template)
    {
        if (record is null) throw new DomainValidationException("The medical record is required and cannot be null.");
        if (newDetail is null) throw new DomainValidationException("The clinical detail is required and cannot be null.");
        if (template is null) throw new DomainValidationException("The clinical form template is required and cannot be null.");

        if (newDetail.TemplateCode != template.Code)
            throw new BusinessRuleValidationException($"The detail template code '{newDetail.TemplateCode}' does not match the provided template '{template.Code}'.");

        if (string.IsNullOrWhiteSpace(newDetail.JsonDataPayload))
            throw new BusinessRuleValidationException($"No data payload provided for template '{template.Code}'.");

        if (!string.IsNullOrWhiteSpace(template.JsonSchemaDefinition) && template.JsonSchemaDefinition is not "{}" && 
            !jsonSchemaValidator.ValidateSchema(template.JsonSchemaDefinition, newDetail.JsonDataPayload, out string? errorMessage))
        {
            throw new BusinessRuleValidationException($"Validation failed for template '{template.Name}': {errorMessage}");
        }

        record.AddClinicalDetail(newDetail);
    }
}
