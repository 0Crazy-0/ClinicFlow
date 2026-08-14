namespace ClinicFlow.Application.Patients.Queries.DTOs;

/// <param name="BloodType">The patient's blood type (e.g. "O+", "AB-").</param>
public sealed record PatientDto(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);
