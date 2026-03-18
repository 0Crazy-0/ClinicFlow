using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CreateCompletePatientProfile;

public class CreateCompletePatientProfileCommandHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateCompletePatientProfileCommand, Guid>
{
    public async Task<Guid> Handle(CreateCompletePatientProfileCommand request, CancellationToken cancellationToken)
    {
        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");

        var patient = Patient.CreateSelf(request.UserId, fullName, request.DateOfBirth);

        var bloodType = BloodType.Create(request.BloodType);
        var emergencyContact = EmergencyContact.Create(request.EmergencyContactName, request.EmergencyContactPhone);

        patient.UpdateMedicalProfile(bloodType, request.Allergies, request.ChronicConditions);
        patient.UpdateEmergencyContact(emergencyContact);

        await patientRepository.CreateAsync(patient, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}