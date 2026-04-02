using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Cancellation;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;

public sealed class CancelAppointmentByPatientCommandHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IMedicalSpecialtyRepository specialtyRepository,
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelAppointmentByPatientCommand>
{
    public async Task Handle(
        CancelAppointmentByPatientCommand request,
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

        var doctor =
            await doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                appointment.DoctorId
            );

        var specialty =
            await specialtyRepository.GetByIdAsync(doctor.MedicalSpecialtyId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalSpecialty),
                doctor.MedicalSpecialtyId
            );

        var initiatorPatient = await patientRepository.GetByUserIdAsync(
            request.InitiatorUserId,
            cancellationToken
        );

        AppointmentCancellationService.CancelByPatient(
            appointment,
            new PatientCancellationArgs
            {
                TargetPatient = targetPatient,
                InitiatorPatient = initiatorPatient,
                Category = appointmentType.Category,
                Specialty = specialty,
                Reason = request.Reason,
                CancelledAt = timeProvider.GetUtcNow().UtcDateTime,
            }
        );

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
