using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
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
    public static MedicalRecord InitiateMedicalRecord(
        Appointment appointment,
        string chiefComplaint
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);

        if (appointment.Status is not Enums.AppointmentStatus.InProgress)
            throw new BusinessRuleValidationException(
                DomainErrors.MedicalEncounter.AppointmentNotInProgress
            );

        return MedicalRecord.Create(
            appointment.PatientId,
            appointment.DoctorId,
            appointment.Id,
            chiefComplaint
        );
    }

    public void ValidateAndCompleteRecord(MedicalRecord record, MedicalEncounterContext context)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ExpectedDoctor);
        ArgumentNullException.ThrowIfNull(context.Appointment);
        ArgumentNullException.ThrowIfNull(context.AppointmentTypeDefinition);

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

    public void AppendClinicalDetail(
        MedicalRecord record,
        DynamicClinicalDetail newDetail,
        ClinicalFormTemplate template
    )
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(newDetail);
        ArgumentNullException.ThrowIfNull(template);

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
