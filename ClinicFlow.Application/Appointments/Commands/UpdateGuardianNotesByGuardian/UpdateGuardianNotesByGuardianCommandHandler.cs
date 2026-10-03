using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.GuardianNotes;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdateGuardianNotesByGuardian;

public sealed class UpdateGuardianNotesByGuardianCommandHandler(
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateGuardianNotesByGuardianCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        UpdateGuardianNotesByGuardianCommand request,
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

        var targetPatient =
            await patientRepository.GetByIdAsync(appointment.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                appointment.PatientId
            );

        AppointmentGuardianNotesService.UpdateByGuardian(
            appointment,
            new UpdateGuardianNotesArgs
            {
                TargetPatient = targetPatient,
                InitiatorMembership = membership,
                InitiatorUserId = request.InitiatorUserId,
                Notes = request.Notes,
            }
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
