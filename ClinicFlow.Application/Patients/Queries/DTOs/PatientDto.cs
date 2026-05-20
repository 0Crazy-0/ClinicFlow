using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Patients.Queries.DTOs;

/// <param name="UserId">The unique identifier of the user account owning this patient profile.</param>
/// <param name="RelationshipToUser">The relationship category between this patient profile and the user account holder.</param>
/// <param name="BloodType">The patient's blood type (e.g. "O+", "AB-").</param>
public sealed record PatientDto(
    Guid Id,
    Guid UserId,
    string FullName,
    PatientRelationship RelationshipToUser,
    DateTime DateOfBirth,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);
