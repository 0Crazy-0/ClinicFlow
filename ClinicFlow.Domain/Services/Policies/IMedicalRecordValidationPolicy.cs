using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;

namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// Defines a dynamic domain rule policy for validating clinical details based on appointment templates.
/// </summary>
public interface IMedicalRecordValidationPolicy
{
    /// <summary>
    /// Validates the provided clinical details against the requirements of the appointment type.
    /// Throws BusinessRuleValidationException if validation fails.
    /// </summary>
    void Validate(
        AppointmentTypeDefinition appointmentType,
        IEnumerable<IClinicalDetailRecord> providedDetails
    );
}
