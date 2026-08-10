using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CreatePatientProfile;

public sealed class CreatePatientProfileCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreatePatientProfileCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        CreatePatientProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");

        return await unitOfWork.ExecuteWithLockAsync(
            request.UserId,
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

                await patientRepository.CreateAsync(patient, cancellationToken);
                await familyMembershipRepository.CreateAsync(membership, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return patient.Id;
            },
            cancellationToken
        );
    }
}
