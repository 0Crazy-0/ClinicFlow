using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.AddAllowedAppointmentCategory;

public sealed class AddAllowedAppointmentCategoryCommandHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IPatientRepository patientRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddAllowedAppointmentCategoryCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        AddAllowedAppointmentCategoryCommand request,
        CancellationToken cancellationToken
    )
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

        var requesterIsPatientsSelf = await familyMembershipRepository.HasActiveSelfMembershipAsync(
            request.RequesterUserId,
            request.PatientId,
            cancellationToken
        );

        var isAuthorized = FamilyMembershipAccessAuthorizationService.CanManageFamilyMembership(
            new FamilyMembershipManagementAuthorizationContext
            {
                Patient = patient,
                ReferenceTime = timeProvider.GetUtcNow().UtcDateTime,
                RequesterUserId = request.RequesterUserId,
                TargetUserId = request.TargetUserId,
                RequesterIsPatientsSelf = requesterIsPatientsSelf,
            }
        );

        membership.AddAllowedAppointmentCategory(request.Category, isAuthorized);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
