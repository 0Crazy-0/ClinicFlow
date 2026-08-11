using ClinicFlow.Application.Common.Utilities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CreateCompletePatientProfile;

public sealed class CreateCompletePatientProfileCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateCompletePatientProfileCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        CreateCompletePatientProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");
        var bloodType = BloodType.Create(request.BloodType);
        var emergencyContact = EmergencyContact.Create(
            request.EmergencyContactName,
            request.EmergencyContactPhone
        );

        var lockKey = DeterministicKeyGenerator.FromComposite(
            fullName.FullName.Trim().ToUpperInvariant(),
            request.DateOfBirth.ToString("yyyy-MM-dd")
        );

        return await unitOfWork.ExecuteWithLockAsync(
            lockKey,
            async cancellationToken =>
            {
                var existingProfile = await patientRepository.GetIncludingDeletedByNameAndDobAsync(
                    fullName,
                    request.DateOfBirth,
                    cancellationToken
                );

                var hasExistingSelfMembership =
                    existingProfile is not null
                    && await familyMembershipRepository.HasActiveSelfMembershipByPatientIdAsync(
                        existingProfile.Id,
                        cancellationToken
                    );

                var (patient, membership) = PrimaryProfileRegistrationService.Register(
                    new PrimaryProfileRegistrationArgs
                    {
                        ExistingPatient = existingProfile,
                        HasExistingSelfMembership = hasExistingSelfMembership,
                        UserId = request.UserId,
                        FullName = fullName,
                        DateOfBirth = request.DateOfBirth,
                        ReferenceTime = timeProvider.GetUtcNow().UtcDateTime,
                    }
                );

                if (existingProfile is null)
                {
                    patient.UpdateMedicalProfile(
                        bloodType,
                        request.Allergies,
                        request.ChronicConditions
                    );

                    patient.UpdateEmergencyContact(emergencyContact);
                }

                await patientRepository.CreateAsync(patient, cancellationToken);
                await familyMembershipRepository.CreateAsync(membership, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return patient.Id;
            },
            cancellationToken
        );
    }
}
