using ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;
using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;

public sealed record AddCompleteFamilyMemberCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string BloodType,
    string Allergies,
    string ChronicConditions,
    string EmergencyContactName,
    string EmergencyContactPhone,
    PatientRelationship Relationship
) : IRequest<Guid>, ICompletePatientCommand;
