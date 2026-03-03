using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Defines a type of appointment with its name, description, and expected duration.
/// </summary>
public class AppointmentTypeDefinition : BaseEntity
{

    public AppointmentType Type { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public TimeSpan DurationMinutes { get; private set; }

    private readonly List<ClinicalFormTemplate> _requiredTemplates = [];

    /// <summary>
    /// The collection of dynamic clinical form templates that must be completed for this appointment type.
    /// </summary>
    public IReadOnlyCollection<ClinicalFormTemplate> RequiredTemplates => _requiredTemplates.AsReadOnly();

    // EF Core constructor
    private AppointmentTypeDefinition() { }

    private AppointmentTypeDefinition(AppointmentType type, string name, string description, TimeSpan durationMinutes)
    {
        Type = type;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
    }

    /// <summary>
    /// Creates a new appointment type definition.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the name is empty or the duration is not positive.</exception>
    internal static AppointmentTypeDefinition Create(AppointmentType type, string name, string description, TimeSpan durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainValidationException("Appointment type name cannot be empty.");
        if (durationMinutes <= TimeSpan.Zero) throw new DomainValidationException("Duration must be positive.");

        return new AppointmentTypeDefinition(type, name, description, durationMinutes);
    }

    /// <summary>
    /// Adds a required clinical form template to this appointment type.
    /// </summary>
    public void AddRequiredTemplate(ClinicalFormTemplate template)
    {
        if (template is null) throw new DomainValidationException("Template cannot be null.");
        if (_requiredTemplates.Any(t => t.Id == template.Id || t.Code == template.Code))
            throw new DomainValidationException($"Template '{template.Code}' is already required for this appointment type.");

        _requiredTemplates.Add(template);
    }

    /// <summary>
    /// Removes a required clinical form template from this appointment type.
    /// </summary>
    public void RemoveRequiredTemplate(ClinicalFormTemplate template)
    {
        if (template is null) return;
        _requiredTemplates.RemoveAll(t => t.Id == template.Id || t.Code == template.Code);
    }
}
