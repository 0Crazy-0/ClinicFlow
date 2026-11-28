using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Patients.Queries.DTOs;

public sealed record PatientDuplicatePreCheckScoreDto(
    Guid PatientId,
    string FullName,
    DateOnly DateOfBirth,
    BiologicalSex SexAtBirth,
    string AddressLine,
    AddressState AddressState,
    string AddressPostalCode,
    string? Email,
    string? Phone,
    int Score
);
