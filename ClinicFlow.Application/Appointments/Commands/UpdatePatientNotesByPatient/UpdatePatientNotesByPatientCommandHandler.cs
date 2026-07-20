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
    IPatientRepository patientRepository,
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

        var targetPatient =
            await patientRepository.GetByIdAsync(appointment.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                appointment.PatientId
            );

        var initiatorPatient =
            await patientRepository.GetSelfPatientByUserIdAsync(
                request.InitiatorUserId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.InitiatorUserId
            );

        PatientAccessService.VerifyAccess(initiatorPatient, targetPatient);

        appointment.UpdatePatientNotes(request.Notes);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
