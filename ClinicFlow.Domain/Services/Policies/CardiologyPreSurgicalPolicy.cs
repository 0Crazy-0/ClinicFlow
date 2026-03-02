using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Services.Policies;

public class CardiologyPreSurgicalPolicy : IMedicalRecordValidationPolicy
{
    public bool AppliesTo(MedicalSpecialty specialty, AppointmentType appointmentType) => specialty.Name.Equals("Cardiology", StringComparison.OrdinalIgnoreCase)
        && appointmentType is AppointmentType.Procedure;


    public void Validate(IEnumerable<ClinicalDetailRecord> providedDetails)
    {
        if (!providedDetails.OfType<CardiologyClinicalDetail>().Any())
            throw new BusinessRuleValidationException("Cardiology details like Blood Pressure are mandatory for cardiology procedures.");

    }
}
