using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.LeaveFamilyMembership;

public sealed class LeaveFamilyMembershipCommandHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IPatientRepository patientRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork
) : IRequestHandler<LeaveFamilyMembershipCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        LeaveFamilyMembershipCommand request,
        CancellationToken cancellationToken
    )
    {
        var membership =
            await familyMembershipRepository.GetActiveMembershipAsync(
                request.UserId,
                request.PatientId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(FamilyMembership),
                request.PatientId
            );

        var referenceTime = timeProvider.GetUtcNow().UtcDateTime;

        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        var memberAge = patient.GetAge(DateOnly.FromDateTime(referenceTime));

        membership.Leave(memberAge, referenceTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
