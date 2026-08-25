using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
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
                var ownerSelfMembership =
                    await familyMembershipRepository.GetActiveSelfMembershipByUserIdAsync(
                        request.UserId,
                        cancellationToken
                    )
                    ?? throw new PrimaryPatientRequiredException(
                        DomainErrors.Patient.PrimaryPatientRequired,
                        request.UserId
                    );

                var referenceTime = timeProvider.GetUtcNow().UtcDateTime;

                var ownerPatient =
                    await patientRepository.GetByIdAsync(
                        ownerSelfMembership.PatientId,
                        cancellationToken
                    )
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(Patient),
                        ownerSelfMembership.PatientId
                    );

                var ownerAge = ownerPatient.GetAge(DateOnly.FromDateTime(referenceTime));

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
                        OwnerAgeInYears = ownerAge,
                        OwnerUserId = request.UserId,
                        Role = request.Relationship,
                        FullName = fullName,
                        DateOfBirth = request.DateOfBirth,
                        ReferenceTime = referenceTime,
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
