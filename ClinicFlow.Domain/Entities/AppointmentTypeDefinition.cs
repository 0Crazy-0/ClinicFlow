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

    /// <summary>
    /// When <c>true</c>, any doctor within the specialty can schedule this type.
    /// <see cref="AllowedSpecialtyIds"/> is ignored.
    /// </summary>
    public bool IsUnrestrictedBySpecialty { get; private set; }

    private readonly HashSet<Guid> _allowedSpecialtyIds = [];

    public IReadOnlyCollection<Guid> AllowedSpecialtyIds => _allowedSpecialtyIds;

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
        AgeEligibilityPolicy agePolicy,
        bool isUnrestrictedBySpecialty
    )
    {
        Category = category;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
        AgePolicy = agePolicy;
        IsUnrestrictedBySpecialty = isUnrestrictedBySpecialty;
    }

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
            agePolicy ?? AgeEligibilityPolicy.NoRestriction,
            true
        );
    }

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

    public void MakeUnrestricted()
    {
        if (IsUnrestrictedBySpecialty)
            throw new DomainValidationException(DomainErrors.AppointmentType.AlreadyUnrestricted);

        _allowedSpecialtyIds.Clear();
        IsUnrestrictedBySpecialty = true;
    }

    public void RestrictToSpecialties(IReadOnlyCollection<Guid> specialtyIds)
    {
        if (!IsUnrestrictedBySpecialty)
            throw new DomainValidationException(DomainErrors.AppointmentType.AlreadyRestricted);

        if (specialtyIds is null || specialtyIds.Count is 0)
            throw new DomainValidationException(
                DomainErrors.AppointmentType.RequiresAtLeastOneSpecialty
            );

        if (specialtyIds.Any(id => id == Guid.Empty))
            throw new DomainValidationException(DomainErrors.Validation.InvalidValue);

        if (specialtyIds.Count != specialtyIds.Distinct().Count())
            throw new DomainValidationException(DomainErrors.Validation.DuplicateValues);

        _allowedSpecialtyIds.Clear();
        _allowedSpecialtyIds.UnionWith(specialtyIds);

        IsUnrestrictedBySpecialty = false;
    }

    public void AddAllowedSpecialty(Guid specialtyId)
    {
        if (IsUnrestrictedBySpecialty)
            throw new DomainValidationException(
                DomainErrors.AppointmentType.CannotAddSpecialtyToGlobalType
            );

        if (specialtyId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (_allowedSpecialtyIds.Contains(specialtyId))
            throw new DomainValidationException(
                DomainErrors.AppointmentType.SpecialtyAlreadyAllowed
            );

        _allowedSpecialtyIds.Add(specialtyId);
    }

    public void RemoveAllowedSpecialty(Guid specialtyId)
    {
        if (IsUnrestrictedBySpecialty)
            throw new DomainValidationException(
                DomainErrors.AppointmentType.CannotRemoveSpecialtyFromGlobalType
            );

        if (!_allowedSpecialtyIds.Contains(specialtyId))
            throw new DomainValidationException(DomainErrors.AppointmentType.SpecialtyNotFound);

        if (_allowedSpecialtyIds.Count is 1)
            throw new DomainValidationException(
                DomainErrors.AppointmentType.RequiresAtLeastOneSpecialty
            );

        _allowedSpecialtyIds.Remove(specialtyId);
    }

    public void ChangeAgePolicy(AgeEligibilityPolicy agePolicy) =>
        AgePolicy = agePolicy ?? AgeEligibilityPolicy.NoRestriction;

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

    public void RemoveRequiredTemplate(ClinicalFormTemplate template)
    {
        if (template is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        var existing =
            _requiredTemplates.Find(t => t.Id == template.Id)
            ?? throw new DomainValidationException(DomainErrors.AppointmentType.TemplateNotFound);

        _requiredTemplates.Remove(existing);
    }

    /// <summary>
    /// Verifies if the patient meets the age requirements and legal guardian requirements for this appointment type.
    /// </summary>
    internal void ValidatePatientEligibility(
        int patientAgeInYears,
        bool hasGuardianConsent = false
    ) => AgePolicy.ValidatePatientEligibility(patientAgeInYears, hasGuardianConsent);
}
