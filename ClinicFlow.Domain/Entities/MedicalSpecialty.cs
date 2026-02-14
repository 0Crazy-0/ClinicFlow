using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.Entities;

public class MedicalSpecialty : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int TypicalDurationMinutes { get; private set; }
    public int MinCancellationHours { get; private set; }

    // EF Core
    private MedicalSpecialty() { }

    private MedicalSpecialty(string name, string description, int typicalDurationMinutes, int minCancellationHours)
    {
        Name = name;
        Description = description;
        TypicalDurationMinutes = typicalDurationMinutes;
        MinCancellationHours = minCancellationHours;
    }

    // Factory Method
    internal static MedicalSpecialty Create(string name, string description, int typicalDurationMinutes, int minCancellationHours)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleValidationException("Specialty name cannot be empty.");
        if (typicalDurationMinutes <= 0) throw new BusinessRuleValidationException("Duration must be positive.");
        if (minCancellationHours < 0) throw new BusinessRuleValidationException("Cancellation hours cannot be negative.");

        return new MedicalSpecialty(name, description, typicalDurationMinutes, minCancellationHours);
    }

    public bool IsCancellationAllowed(DateTime appointmentDateTime)
    {
        var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;
        return hoursUntilAppointment >= MinCancellationHours;
    }
}
