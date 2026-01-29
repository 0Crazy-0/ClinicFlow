using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Entities;

public class MedicalSpecialty : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int TypicalDurationMinutes { get; private set; }
    public int MinCancellationHours { get; private set; }
}
