namespace ClinicFlow.Application.MedicalSpecialties.Queries.DTOs;

public sealed record MedicalSpecialtyDto(
    Guid Id,
    string Name,
    string Description,
    int TypicalDurationMinutes,
    int MinCancellationHours,
    bool IsDeleted
);
