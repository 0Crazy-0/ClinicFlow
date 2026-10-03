using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
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

        appointment.UpdatePatientNotes(request.Notes);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
