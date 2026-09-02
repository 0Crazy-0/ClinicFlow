using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.ChangeAccessLevel;

public sealed class ChangeAccessLevelCommandHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IPatientRepository patientRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork
) : IRequestHandler<ChangeAccessLevelCommand>
{
    /// <inheritdoc />
    public async Task Handle(ChangeAccessLevelCommand request, CancellationToken cancellationToken)
    {
        var membership =
            await familyMembershipRepository.GetActiveMembershipAsync(
                request.TargetUserId,
                request.PatientId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(FamilyMembership),
                request.PatientId
            );

        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        var requesterHasSelfMembership =
            await familyMembershipRepository.HasActiveSelfMembershipByUserIdAsync(
                request.RequesterUserId,
                cancellationToken
            );

        var requesterMembership = await familyMembershipRepository.GetActiveMembershipAsync(
            request.RequesterUserId,
            request.PatientId,
            cancellationToken
        );

        var isAuthorized = FamilyMembershipAccessAuthorizationService.CanChangeAccessLevel(
            new AccessLevelChangeAuthorizationContext
            {
                Patient = patient,
                ReferenceTime = timeProvider.GetUtcNow().UtcDateTime,
                RequesterHasSelfMembership = requesterHasSelfMembership,
                RequesterIsPatientsSelf = requesterMembership?.Role is PatientRelationship.Self,
                RequesterHasActiveMembershipWithPatient = requesterMembership is not null,
            }
        );

        membership.ChangeAccessLevel(request.NewAccessLevel, isAuthorized);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
