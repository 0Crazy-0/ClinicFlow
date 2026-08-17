using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.RevokeFamilyMember;

public sealed class RevokeFamilyMemberCommandHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IAppointmentRepository appointmentRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork
) : IRequestHandler<RevokeFamilyMemberCommand>
{
    /// <inheritdoc />
    public async Task Handle(RevokeFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.ExecuteWithLockAsync(
            request.OwnerUserId,
            async cancellationToken =>
            {
                var membership =
                    await familyMembershipRepository.GetActiveMembershipAsync(
                        request.OwnerUserId,
                        request.PatientId,
                        cancellationToken
                    )
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(FamilyMembership),
                        request.PatientId
                    );

                var patientHasOwnSelfMembership =
                    await familyMembershipRepository.HasActiveSelfMembershipByPatientIdAsync(
                        request.PatientId,
                        cancellationToken
                    );

                var hasUpcomingAppointmentRequiringGuardianForMinor =
                    await appointmentRepository.HasUpcomingAppointmentRequiringGuardianForMinorAsync(
                        request.PatientId,
                        timeProvider.GetUtcNow().UtcDateTime,
                        cancellationToken
                    );

                membership.Revoke(
                    patientHasOwnSelfMembership,
                    hasUpcomingAppointmentRequiringGuardianForMinor,
                    timeProvider.GetUtcNow().UtcDateTime
                );

                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken
        );
    }
}
