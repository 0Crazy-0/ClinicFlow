using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Patients.Queries.DTOs;

public record PatientDto(Guid Id, Guid UserId, string FullName, PatientRelationship RelationshipToUser, DateTime DateOfBirth,
    string? BloodType, string? Allergies, string? ChronicConditions, string? EmergencyContactName, string? EmergencyContactPhone);
