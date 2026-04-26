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
    /// Executes dynamic validation policies against the clinical details, populates the medical record,
    /// and advances the appointment lifecycle status to Completed.
    /// </summary>
    public void ValidateAndCompleteRecord(MedicalRecord record, MedicalEncounterContext context)
    {
        if (record is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (
            context is null
            || context.ExpectedDoctor is null
            || context.Appointment is null
            || context.AppointmentTypeDefinition is null
        )
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
    /// Enforces that the clinical detail's JSON structure complies with the template schema before appending it to the record.
    /// </summary>
    public void AppendClinicalDetail(
        MedicalRecord record,
        IClinicalDetailRecord newDetail,
        ClinicalFormTemplate template
    )
    {
        if (record is null || newDetail is null || template is null)
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
