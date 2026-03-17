using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.AddFamilyMember;

public class AddFamilyMemberCommandHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork) : IRequestHandler<AddFamilyMemberCommand, Guid>
{
    public async Task<Guid> Handle(AddFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");
        var bloodType = BloodType.Create(request.BloodType);
        var emergencyContact = EmergencyContact.Create(request.EmergencyContactName, request.EmergencyContactPhone);

        var familyMember = Patient.CreateFamilyMember(request.UserId, fullName, request.Relationship, request.DateOfBirth, bloodType,
            request.Allergies, request.ChronicConditions, emergencyContact);

        await patientRepository.CreateAsync(familyMember, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return familyMember.Id;
    }
}
