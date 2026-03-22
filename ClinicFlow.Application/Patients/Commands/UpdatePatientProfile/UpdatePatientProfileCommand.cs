using MediatR;

namespace ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;

public record UpdatePatientProfileCommand(
    Guid PatientId,
    string BloodType,
    string Allergies,
    string ChronicConditions,
    string EmergencyContactName,
    string EmergencyContactPhone
) : IRequest;
