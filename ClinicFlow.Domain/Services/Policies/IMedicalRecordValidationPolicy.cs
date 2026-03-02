using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// Defines a domain rule policy for validating clinical details based on specialty and appointment type.
/// </summary>
public interface IMedicalRecordValidationPolicy
{
    /// <summary>
    /// Checks if this policy applies to the given specialty and appointment type.
    /// </summary>
    bool AppliesTo(MedicalSpecialty specialty, AppointmentType appointmentType);

    /// <summary>
    /// Validates the provided clinical details against the business rules of this policy.
    /// Throws DomainRuleException if validation fails.
    /// </summary>
    void Validate(IEnumerable<IClinicalDetailRecord> providedDetails);
}
