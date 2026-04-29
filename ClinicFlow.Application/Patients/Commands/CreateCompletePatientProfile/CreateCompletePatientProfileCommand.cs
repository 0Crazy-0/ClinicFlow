using ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CreateCompletePatientProfile;

public sealed record CreateCompletePatientProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string BloodType,
    string Allergies,
    string ChronicConditions,
    string EmergencyContactName,
    string EmergencyContactPhone
) : IRequest<Guid>, ICompletePatientCommand;
