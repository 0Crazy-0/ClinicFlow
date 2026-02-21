using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

public class AppointmentTypeDefinition : BaseEntity
{
    public AppointmentType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TimeSpan DurationMinutes { get; private set; }

    // EF Core constructor
    private AppointmentTypeDefinition() { }

    private AppointmentTypeDefinition(AppointmentType type, string name, string description, TimeSpan durationMinutes)
    {
        Type = type;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
    }

    // Factory Method
    internal static AppointmentTypeDefinition Create(AppointmentType type, string name, string description, TimeSpan durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainValidationException("Appointment type name cannot be empty.");
        if (durationMinutes <= TimeSpan.Zero) throw new DomainValidationException("Duration must be positive.");

        return new AppointmentTypeDefinition(type, name, description, durationMinutes);
    }
}
