using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByPatient;

public sealed class UpdatePatientNotesByPatientCommandHandler(
    IAppointmentRepository appointmentRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdatePatientNotesByPatientCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        UpdatePatientNotesByPatientCommand request,
        CancellationToken cancellationToken
    )
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var hasAccess = await familyMembershipRepository.HasActiveMembershipAsync(
            request.InitiatorUserId,
            appointment.PatientId,
            cancellationToken
        );

        PatientAccessService.VerifyAccess(hasAccess);

        appointment.UpdatePatientNotes(request.Notes);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
