using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Services.Policies;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service responsible for orchestrating the rules around a medical encounter.
/// </summary>
public class MedicalEncounterService(
    IEnumerable<IMedicalRecordValidationPolicy> policies,
    IJsonSchemaValidator jsonSchemaValidator
)
{
    /// <summary>
    /// Validates business rules against the provided context and updates the medical record with the given details.
    /// </summary>
    /// <param name="record">The medical record to be validated and updated.</param>
    /// <param name="context">The contextual information for the encounter, including expected references (Doctor, Appointment) and new clinical details.</param>
    public void ValidateAndCompleteRecord(MedicalRecord record, MedicalEncounterContext context)
    {
        if (record is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
        if (context is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
        if (context.ExpectedDoctor is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
        if (context.Appointment is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
        if (context.AppointmentTypeDefinition is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (record.DoctorId != context.ExpectedDoctor.Id)
            throw new BusinessRuleValidationException(DomainErrors.MedicalEncounter.DoctorMismatch);

        if (record.AppointmentId != context.Appointment.Id)
            throw new BusinessRuleValidationException(
                DomainErrors.MedicalEncounter.AppointmentMismatch
            );

        foreach (var policy in policies)
            policy.Validate(context.AppointmentTypeDefinition, context.ProvidedDetails);

        foreach (var detail in context.ProvidedDetails)
            record.AddClinicalDetail(detail);

        context.Appointment.Complete(context.CompletedAt);
    }

    /// <summary>
    /// Validates and appends a single clinical detail to an existing medical record.
    /// Enforces that the detail's JSON structure complies with the template schema if one exists.
    /// </summary>
    /// <param name="record">The medical record where the detail will be appended.</param>
    /// <param name="newDetail">The new clinical detail containing the data payload to validate and append.</param>
    /// <param name="template">The clinical form template containing the schema requirements.</param>
    public void AppendClinicalDetail(
        MedicalRecord record,
        IClinicalDetailRecord newDetail,
        ClinicalFormTemplate template
    )
    {
        if (record is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
        if (newDetail is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
        if (template is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (newDetail.TemplateCode != template.Code)
            throw new BusinessRuleValidationException(DomainErrors.MedicalEncounter.CodeMismatch);

        if (string.IsNullOrWhiteSpace(newDetail.JsonDataPayload))
            throw new BusinessRuleValidationException(DomainErrors.MedicalEncounter.MissingPayload);

        if (
            !string.IsNullOrWhiteSpace(template.JsonSchemaDefinition)
            && template.JsonSchemaDefinition is not "{}"
            && !jsonSchemaValidator.ValidateSchema(
                template.JsonSchemaDefinition,
                newDetail.JsonDataPayload,
                out string? errorMessage
            )
        )
        {
            throw new BusinessRuleValidationException(
                $"{DomainErrors.MedicalEncounter.ValidationFailed}: {errorMessage}"
            );
        }

        record.AddClinicalDetail(newDetail);
    }
}
