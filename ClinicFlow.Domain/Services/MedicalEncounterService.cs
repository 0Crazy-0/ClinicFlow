using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service responsible for orchestrating the rules around a medical encounter.
/// </summary>
public class MedicalEncounterService(IEnumerable<IMedicalRecordValidationPolicy> policies)
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
}
