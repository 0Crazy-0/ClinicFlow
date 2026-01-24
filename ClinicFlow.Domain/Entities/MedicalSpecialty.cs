using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Entities;

public class MedicalSpecialty : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TypicalDurationMinutes { get; set; }
    public int MinCancellationHours { get; set; }

    public ICollection<Doctor> Doctors { get; set; }

    public MedicalSpecialty()
    {
        Doctors = [];
    }
}
