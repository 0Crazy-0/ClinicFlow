using MediatR;

namespace ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;

public sealed record UpdatePatientProfileCommand(
    Guid PatientId,
    string BloodType,
    string Allergies,
    string ChronicConditions,
    string EmergencyContactFirstName,
    string? EmergencyContactMiddleName,
    string EmergencyContactLastName,
    string EmergencyContactPhone
) : IRequest;
