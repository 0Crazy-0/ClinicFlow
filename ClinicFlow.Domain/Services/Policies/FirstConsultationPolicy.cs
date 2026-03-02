using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Services.Policies;

public class FirstConsultationPolicy : IMedicalRecordValidationPolicy
{
    public bool AppliesTo(MedicalSpecialty specialty, AppointmentType appointmentType) => appointmentType is AppointmentType.FirstConsultation;

    public void Validate(IEnumerable<ClinicalDetailRecord> providedDetails)
    {
        if (!providedDetails.OfType<FirstVisitDetail>().Any())
            throw new BusinessRuleValidationException("First visit details (e.g. Family History, Allergies) are mandatory for the first consultation.");

    }
}
