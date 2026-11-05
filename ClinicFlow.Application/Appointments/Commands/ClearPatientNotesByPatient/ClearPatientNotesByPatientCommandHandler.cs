using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ClearPatientNotesByPatient;

public sealed class ClearPatientNotesByPatientCommandHandler(
    IAppointmentRepository appointmentRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ClearPatientNotesByPatientCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        ClearPatientNotesByPatientCommand request,
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

        var membership =
            await familyMembershipRepository.GetActiveMembershipAsync(
                request.InitiatorUserId,
                appointment.PatientId,
                cancellationToken
            )
            ?? throw new PatientAccessUnauthorizedException(
                DomainErrors.Patient.UnauthorizedAccess
            );

        membership.EnsurePatientNotesWrite();

        appointment.ClearPatientNotes();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
