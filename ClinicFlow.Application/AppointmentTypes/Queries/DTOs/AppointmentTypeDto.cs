using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;

namespace ClinicFlow.Application.AppointmentTypes.Queries.DTOs;

/// <param name="Category">The category of the appointment (e.g. Consultation, Procedure).</param>
/// <param name="MinimumAge">The optional minimum age required to schedule this appointment type.</param>
/// <param name="MaximumAge">The optional maximum age allowed to schedule this appointment type.</param>
/// <param name="RequiresLegalGuardian">A value indicating whether a legal guardian is required due to patient age policies.</param>
public sealed record AppointmentTypeDto(
    Guid Id,
    string Category,
    string Name,
    string Description,
    int DurationMinutes,
    int? MinimumAge,
    int? MaximumAge,
    bool RequiresLegalGuardian,
    bool IsUnrestrictedBySpecialty,
    IReadOnlyCollection<Guid> AllowedSpecialtyIds,
    IReadOnlyCollection<ClinicalFormTemplateDto> RequiredTemplates
);
