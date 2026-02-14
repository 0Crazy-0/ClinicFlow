using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.Entities;

public class AppointmentType : BaseEntity
{
    public AppointmentTypeEnum Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TimeSpan DurationMinutes { get; private set; }

    // EF Core constructor
    private AppointmentType() { }

    private AppointmentType(AppointmentTypeEnum type, string name, string description, TimeSpan durationMinutes)
    {
        Type = type;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
    }

    // Factory Method
    internal static AppointmentType Create(AppointmentTypeEnum type, string name, string description, TimeSpan durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleValidationException("Appointment type name cannot be empty.");
        if (durationMinutes <= TimeSpan.Zero) throw new BusinessRuleValidationException("Duration must be positive.");

        return new AppointmentType(type, name, description, durationMinutes);
    }
}
