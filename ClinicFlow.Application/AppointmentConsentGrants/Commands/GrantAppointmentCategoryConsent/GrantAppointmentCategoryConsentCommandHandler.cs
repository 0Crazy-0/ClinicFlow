using ClinicFlow.Application.Common.Utilities;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts.Consent;
using MediatR;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantAppointmentCategoryConsent;

public sealed class GrantAppointmentCategoryConsentCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IAppointmentConsentGrantRepository consentGrantRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<GrantAppointmentCategoryConsentCommand, Guid>
{
    public async Task<Guid> Handle(
        GrantAppointmentCategoryConsentCommand request,
        CancellationToken cancellationToken
    )
    {
        var lockKey = DeterministicKeyGenerator.FromComposite(
            request.PatientId.ToString(),
            $"{request.RecipientMembershipId}{request.ProtectedCareCategory}"
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

                var hasEffectiveGrant = await consentGrantRepository.HasEffectiveCategoryGrantAsync(
                    request.PatientId,
                    request.RecipientMembershipId,
                    request.ProtectedCareCategory,
                    referenceTime,
                    cancellationToken
                );

                var grant = AppointmentConsentGrantService.GrantCategory(
                    new AppointmentCategoryConsentGrantContext
                    {
                        TargetPatient = patient,
                        RequesterMembership = requesterMembership,
                        RecipientMembership = recipientMembership,
                        ProtectedCareCategory = request.ProtectedCareCategory,
                        HasEffectiveGrant = hasEffectiveGrant,
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
