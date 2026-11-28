namespace ClinicFlow.Application.Patients.Queries.DTOs;

public sealed record PatientDuplicatePreCheckDto(
    IReadOnlyList<PatientDuplicatePreCheckScoreDto> Scores,
    int MaxScore,
    bool IsHighBand
);
