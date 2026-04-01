using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;

public sealed class RescheduleByPatientCommandHandler(
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    IScheduleRepository scheduleRepository,
    IPatientPenaltyRepository penaltyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RescheduleByPatientCommand>
{
    public async Task Handle(
        RescheduleByPatientCommand request,
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
            await patientRepository.GetByUserIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.InitiatorUserId
            );

        var penalties = await penaltyRepository.GetByPatientIdAsync(
            appointment.PatientId,
            cancellationToken
        );

        var newTimeRange = TimeRange.Create(request.NewStartTime, request.NewEndTime);

        var doctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(
            appointment.DoctorId,
            request.NewDate.DayOfWeek,
            cancellationToken
        );

        var hasConflict = await appointmentRepository.HasConflictAsync(
            appointment.DoctorId,
            request.NewDate,
            newTimeRange,
            cancellationToken
        );

        AppointmentReschedulingService.RescheduleByPatient(
            appointment,
            new PatientReschedulingArgs
            {
                TargetPatient = targetPatient,
                InitiatorPatient = initiatorPatient,
                NewDate = request.NewDate,
                NewTimeRange = newTimeRange,
            },
            new AppointmentReschedulingContext
            {
                Penalties = penalties,
                DoctorSchedule = doctorSchedule,
                HasConflict = hasConflict,
            }
        );

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
