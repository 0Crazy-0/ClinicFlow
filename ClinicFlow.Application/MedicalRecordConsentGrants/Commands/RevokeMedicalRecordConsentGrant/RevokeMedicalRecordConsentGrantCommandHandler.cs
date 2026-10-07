using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecordConsentGrants.Commands.RevokeMedicalRecordConsentGrant;

public sealed class RevokeMedicalRecordConsentGrantCommandHandler(
    TimeProvider timeProvider,
    IMedicalRecordConsentGrantRepository consentGrantRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RevokeMedicalRecordConsentGrantCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        RevokeMedicalRecordConsentGrantCommand request,
        CancellationToken cancellationToken
    )
    {
        var grant =
            await consentGrantRepository.GetByIdAsync(request.ConsentGrantId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalRecordConsentGrant),
                request.ConsentGrantId
            );

        var requesterIsAuthorized = await familyMembershipRepository.HasActiveSelfMembershipAsync(
            request.RequesterUserId,
            grant.PatientId,
            cancellationToken
        );

        grant.Revoke(requesterIsAuthorized, timeProvider.GetLocalNow().DateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
