namespace ClinicFlow.Domain.Entities.ClinicalDetails;

public class FirstVisitDetail : IClinicalDetailRecord
{
    public string FamilyMedicalHistory { get; private set; } = string.Empty;

    public IList<string> Allergies { get; private set; } = [];

    // EF Core constructor
    private FirstVisitDetail() { }

    public FirstVisitDetail(string familyHistory, List<string> allergies)
    {
        FamilyMedicalHistory = familyHistory ?? string.Empty;
        Allergies = allergies ?? [];
    }
}
