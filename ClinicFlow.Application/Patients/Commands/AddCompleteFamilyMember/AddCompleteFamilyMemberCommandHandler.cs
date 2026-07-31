using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;

public sealed class AddCompleteFamilyMemberCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddCompleteFamilyMemberCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        AddCompleteFamilyMemberCommand request,
        CancellationToken cancellationToken
    )
    {
        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");
        var bloodType = BloodType.Create(request.BloodType);
        var emergencyContact = EmergencyContact.Create(
            request.EmergencyContactName,
            request.EmergencyContactPhone
        );

        return await unitOfWork.ExecuteWithLockAsync(
            request.UserId,
            async cancellationToken =>
            {
                var existingProfile = await patientRepository.GetIncludingDeletedByNameAndDobAsync(
                    request.UserId,
                    fullName,
                    request.DateOfBirth,
                    cancellationToken
                );

                var activeCount = await patientRepository.CountActiveFamilyMembersAsync(
                    request.UserId,
                    cancellationToken
                );

                var patient = FamilyMemberRegistrationService.Register(
                    existingProfile,
                    activeCount,
                    new FamilyMemberRegistrationArgs
                    {
                        UserId = request.UserId,
                        FullName = fullName,
                        Relationship = request.Relationship,
                        DateOfBirth = request.DateOfBirth,
                        ReferenceTime = timeProvider.GetUtcNow().UtcDateTime,
                    }
                );

                patient.UpdateMedicalProfile(
                    bloodType,
                    request.Allergies,
                    request.ChronicConditions
                );
                patient.UpdateEmergencyContact(emergencyContact);

                if (existingProfile is null)
                    await patientRepository.CreateAsync(patient, cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return patient.Id;
            },
            cancellationToken
        );
    }
}
