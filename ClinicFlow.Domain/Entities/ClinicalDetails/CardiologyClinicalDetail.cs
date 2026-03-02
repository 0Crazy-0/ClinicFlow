using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities.ClinicalDetails;

public class CardiologyClinicalDetail : ClinicalDetailRecord
{
    public int SystolicBloodPressure { get; private set; }
    public int DiastolicBloodPressure { get; private set; }
    public bool IrregularHeartbeat { get; private set; }

    // EF Core constructor
    private CardiologyClinicalDetail() { }

    public CardiologyClinicalDetail(int systolic, int diastolic, bool irregularHeartbeat)
    {
        if (systolic <= 0) throw new DomainValidationException("Systolic blood pressure must be positive.");
        if (diastolic <= 0) throw new DomainValidationException("Diastolic blood pressure must be positive.");

        SystolicBloodPressure = systolic;
        DiastolicBloodPressure = diastolic;
        IrregularHeartbeat = irregularHeartbeat;
    }
}
