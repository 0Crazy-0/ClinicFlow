using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Defines a type of appointment with its name, description, and expected duration.
/// </summary>
public class AppointmentTypeDefinition : BaseEntity
{
    public AppointmentCategory Category { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public TimeSpan DurationMinutes { get; private set; }

    public AgeEligibilityPolicy AgePolicy { get; private set; }

    private readonly List<ClinicalFormTemplate> _requiredTemplates = [];

    /// <summary>
    /// The collection of dynamic clinical form templates that must be completed for this appointment type.
    /// </summary>
    public IReadOnlyCollection<ClinicalFormTemplate> RequiredTemplates =>
        _requiredTemplates.AsReadOnly();

    // EF Core constructor
    private AppointmentTypeDefinition()
    {
        AgePolicy = null!;
    }

    private AppointmentTypeDefinition(
        AppointmentCategory category,
        string name,
        string description,
        TimeSpan durationMinutes,
        AgeEligibilityPolicy agePolicy
    )
    {
        Category = category;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
        AgePolicy = agePolicy;
    }

    /// <summary>
    /// Creates a new appointment type definition.
    /// </summary>
    public static AppointmentTypeDefinition Create(
        AppointmentCategory category,
        string name,
        string description,
        TimeSpan durationMinutes,
        AgeEligibilityPolicy? agePolicy = null
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (durationMinutes <= TimeSpan.Zero)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        return new AppointmentTypeDefinition(
            category,
            name,
            description,
            durationMinutes,
            agePolicy ?? AgeEligibilityPolicy.NoRestriction
        );
    }

    /// <summary>
    /// Updates the appointment type's general details.
    /// </summary>
    public void UpdateDetails(
        AppointmentCategory category,
        string name,
        string description,
        TimeSpan durationMinutes
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (durationMinutes <= TimeSpan.Zero)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        Category = category;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
    }

    /// <summary>
    /// Replaces the current age eligibility policy for this appointment type.
    /// </summary>
    public void ChangeAgePolicy(AgeEligibilityPolicy agePolicy) =>
        AgePolicy = agePolicy ?? AgeEligibilityPolicy.NoRestriction;

    /// <summary>
    /// Adds a required clinical form template to this appointment type.
    /// </summary>
    public void AddRequiredTemplate(ClinicalFormTemplate template)
    {
        if (template is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);
        if (_requiredTemplates.Any(t => t.Id == template.Id || t.Code == template.Code))
            throw new DomainValidationException(
                DomainErrors.AppointmentType.TemplateAlreadyRequired
            );

        _requiredTemplates.Add(template);
    }

    /// <summary>
    /// Removes a required clinical form template from this appointment type.
    /// </summary>
    public void RemoveRequiredTemplate(ClinicalFormTemplate template)
    {
        if (template is null)
            return;
        _requiredTemplates.RemoveAll(t => t.Id == template.Id || t.Code == template.Code);
    }

    /// <summary>
    /// Verifies if the patient meets the age requirements and legal guardian requirements for this appointment type.
    /// </summary>
    internal void ValidatePatientEligibility(
        int patientAgeInYears,
        bool hasGuardianConsent = false
    ) => AgePolicy.ValidatePatientEligibility(patientAgeInYears, hasGuardianConsent);
}
