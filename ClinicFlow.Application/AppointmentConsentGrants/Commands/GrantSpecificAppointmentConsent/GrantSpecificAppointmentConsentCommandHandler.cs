using ClinicFlow.Application.Common.Utilities;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using MediatR;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantSpecificAppointmentConsent;

public sealed class GrantSpecificAppointmentConsentCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IAppointmentRepository appointmentRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IAppointmentConsentGrantRepository consentGrantRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<GrantSpecificAppointmentConsentCommand, Guid>
{
    public async Task<Guid> Handle(
        GrantSpecificAppointmentConsentCommand request,
        CancellationToken cancellationToken
    )
    {
        var lockKey = DeterministicKeyGenerator.FromComposite(
            request.PatientId.ToString(),
            $"{request.RecipientMembershipId}{request.AppointmentId}"
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

                var appointment =
                    await appointmentRepository.GetByIdAsync(
                        request.AppointmentId,
                        cancellationToken
                    )
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(Appointment),
                        request.AppointmentId
                    );

                var appointmentType =
                    await appointmentTypeRepository.GetByIdAsync(
                        appointment.AppointmentTypeId,
                        cancellationToken
                    )
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(AppointmentTypeDefinition),
                        appointment.AppointmentTypeId
                    );

                var referenceTime = timeProvider.GetLocalNow().DateTime;

                var hasEffectiveGrant =
                    await consentGrantRepository.HasEffectiveAppointmentGrantAsync(
                        request.PatientId,
                        request.RecipientMembershipId,
                        request.AppointmentId,
                        referenceTime,
                        cancellationToken
                    );

                var grant = AppointmentConsentGrantService.GrantAppointment(
                    new SpecificAppointmentConsentGrantContext
                    {
                        TargetPatient = patient,
                        RequesterMembership = requesterMembership,
                        RecipientMembership = recipientMembership,
                        TargetAppointment = appointment,
                        AppointmentType = appointmentType,
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
