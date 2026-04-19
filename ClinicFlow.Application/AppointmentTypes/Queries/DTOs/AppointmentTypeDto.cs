namespace ClinicFlow.Application.AppointmentTypes.Queries.DTOs;

public sealed record AppointmentTypeDto(
    Guid Id,
    string Category,
    string Name,
    string Description,
    TimeSpan DurationMinutes,
    int? MinimumAge,
    int? MaximumAge,
    bool RequiresLegalGuardian
);
