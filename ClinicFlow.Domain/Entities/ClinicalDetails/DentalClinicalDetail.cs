using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities.ClinicalDetails;

public class DentalClinicalDetail : ClinicalDetailRecord
{
    public string Odontogram { get; private set; } = string.Empty;
    public bool HasGingivitis { get; private set; }
    
    public string TreatedTeethNotes { get; private set; } = string.Empty;

    // EF Core constructor
    private DentalClinicalDetail() { }

    public DentalClinicalDetail(string odontogram, bool hasGingivitis, string treatedTeethNotes)
    {
        if (string.IsNullOrWhiteSpace(odontogram)) throw new DomainValidationException("Odontogram data is required for dental records.");

        Odontogram = odontogram;
        HasGingivitis = hasGingivitis;
        TreatedTeethNotes = treatedTeethNotes ?? string.Empty;
    }
}
