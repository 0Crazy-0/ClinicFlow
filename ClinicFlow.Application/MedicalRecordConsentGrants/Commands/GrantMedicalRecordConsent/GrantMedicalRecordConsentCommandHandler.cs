using ClinicFlow.Application.Common.Utilities;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Consent;
using ClinicFlow.Domain.Services.Contexts;
using MediatR;

namespace ClinicFlow.Application.MedicalRecordConsentGrants.Commands.GrantMedicalRecordConsent;

public sealed class GrantMedicalRecordConsentCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IMedicalRecordRepository medicalRecordRepository,
    IMedicalRecordConsentGrantRepository consentGrantRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<GrantMedicalRecordConsentCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        GrantMedicalRecordConsentCommand request,
        CancellationToken cancellationToken
    )
    {
        var lockKey = DeterministicKeyGenerator.FromComposite(
            request.MedicalRecordId.ToString(),
            request.RecipientMembershipId.ToString()
        );

        return await unitOfWork.ExecuteWithLockAsync(
            lockKey,
            async cancellationToken =>
            {
                var patient =
                    await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(Patient),
                        request.PatientId
                    );

                var requesterMembership =
                    await familyMembershipRepository.GetActiveMembershipAsync(
                        request.RequesterUserId,
                        request.PatientId,
                        cancellationToken
                    )
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(FamilyMembership),
                        request.PatientId
                    );

                var record =
                    await medicalRecordRepository.GetByIdAsync(
                        request.MedicalRecordId,
                        cancellationToken
                    )
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(MedicalRecord),
                        request.MedicalRecordId
                    );

                var recipientMembership =
                    await familyMembershipRepository.GetByIdAsync(
                        request.RecipientMembershipId,
                        cancellationToken
                    )
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(FamilyMembership),
                        request.RecipientMembershipId
                    );

                var referenceTime = timeProvider.GetLocalNow().DateTime;

                var hasEffectiveGrant = await consentGrantRepository.HasEffectiveGrantAsync(
                    request.MedicalRecordId,
                    request.RecipientMembershipId,
                    referenceTime,
                    cancellationToken
                );

                var grant = MedicalRecordConsentGrantService.Grant(
                    new MedicalRecordConsentGrantContext
                    {
                        Patient = patient,
                        RequesterMembership = requesterMembership,
                        MedicalRecord = record,
                        RecipientMembership = recipientMembership,
                        HasEffectiveGrant = hasEffectiveGrant,
                    },
                    new GrantMedicalRecordConsentArgs
                    {
                        RequesterUserId = request.RequesterUserId,
                        ReferenceTime = referenceTime,
                    }
                );

                await consentGrantRepository.CreateAsync(grant, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return grant.Id;
            },
            cancellationToken
        );
    }
}
