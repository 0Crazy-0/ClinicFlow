using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;

public sealed class AddCompleteFamilyMemberCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddCompleteFamilyMemberCommand, Guid>
{
    public async Task<Guid> Handle(AddCompleteFamilyMemberCommand request, CancellationToken ct)
    {
        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");

        var familyMember = Patient.CreateFamilyMember(
            request.UserId,
            fullName,
            request.Relationship,
            request.DateOfBirth,
            timeProvider.GetUtcNow().UtcDateTime
        );

        var bloodType = BloodType.Create(request.BloodType);
        var emergencyContact = EmergencyContact.Create(
            request.EmergencyContactName,
            request.EmergencyContactPhone
        );

        familyMember.UpdateMedicalProfile(bloodType, request.Allergies, request.ChronicConditions);
        familyMember.UpdateEmergencyContact(emergencyContact);

        await patientRepository.CreateAsync(familyMember, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return familyMember.Id;
    }
}
