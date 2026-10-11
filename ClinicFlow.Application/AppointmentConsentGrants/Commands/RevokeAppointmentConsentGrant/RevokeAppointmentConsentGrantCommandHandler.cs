using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.RevokeAppointmentConsentGrant;

public sealed class RevokeAppointmentConsentGrantCommandHandler(
    TimeProvider timeProvider,
    IAppointmentConsentGrantRepository consentGrantRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RevokeAppointmentConsentGrantCommand>
{
    public async Task Handle(
        RevokeAppointmentConsentGrantCommand request,
        CancellationToken cancellationToken
    )
    {
        var grant =
            await consentGrantRepository.GetByIdAsync(request.ConsentGrantId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentConsentGrant),
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
