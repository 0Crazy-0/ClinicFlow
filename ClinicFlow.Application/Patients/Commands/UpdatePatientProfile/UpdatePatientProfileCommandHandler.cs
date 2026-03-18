using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;

public class UpdatePatientProfileCommandHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdatePatientProfileCommand>
{
    public async Task Handle(UpdatePatientProfileCommand request, CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken) ?? 
            throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(Patient), request.PatientId);

        var bloodType = BloodType.Create(request.BloodType);
        var emergencyContact = EmergencyContact.Create(request.EmergencyContactName, request.EmergencyContactPhone);

        patient.UpdateMedicalProfile(bloodType, request.Allergies, request.ChronicConditions);
        patient.UpdateEmergencyContact(emergencyContact);

        await patientRepository.UpdateAsync(patient, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
