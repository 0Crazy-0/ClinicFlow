using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Services.Policies;

public class DentalProcedurePolicy : IMedicalRecordValidationPolicy
{
    public bool AppliesTo(MedicalSpecialty specialty, AppointmentType appointmentType) => specialty.Name.Equals("Dentistry", StringComparison.OrdinalIgnoreCase)
        && appointmentType is AppointmentType.Procedure;

    public void Validate(IEnumerable<IClinicalDetailRecord> providedDetails)
    {
        if (!providedDetails.OfType<DentalClinicalDetail>().Any()) throw new BusinessRuleValidationException("A complete odontogram is mandatory for dental procedures.");
    }
}
