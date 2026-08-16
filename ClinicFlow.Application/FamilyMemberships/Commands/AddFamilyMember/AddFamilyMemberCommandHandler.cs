using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.AddFamilyMember;

public sealed class AddFamilyMemberCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddFamilyMemberCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        AddFamilyMemberCommand request,
        CancellationToken cancellationToken
    )
    {
        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");

        return await unitOfWork.ExecuteWithLockAsync(
            request.UserId,
            async cancellationToken =>
            {
                if (
                    !await familyMembershipRepository.HasActiveSelfMembershipByUserIdAsync(
                        request.UserId,
                        cancellationToken
                    )
                )
                {
                    throw new PrimaryPatientRequiredException(
                        DomainErrors.Patient.PrimaryPatientRequired,
                        request.UserId
                    );
                }

                var existingProfile = await patientRepository.GetByNameAndDobAsync(
                    fullName,
                    request.DateOfBirth,
                    cancellationToken
                );

                var activeCount = await familyMembershipRepository.CountActiveFamilyMembersAsync(
                    request.UserId,
                    cancellationToken
                );

                var hasExistingMembershipWithOwner =
                    existingProfile is not null
                    && await familyMembershipRepository.HasActiveMembershipAsync(
                        request.UserId,
                        existingProfile.Id,
                        cancellationToken
                    );

                var (patient, membership) = FamilyMemberRegistrationService.Register(
                    new FamilyMemberRegistrationArgs
                    {
                        ExistingPatient = existingProfile,
                        HasExistingMembershipWithOwner = hasExistingMembershipWithOwner,
                        ActiveFamilyMemberCount = activeCount,
                        OwnerUserId = request.UserId,
                        Role = request.Relationship,
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
