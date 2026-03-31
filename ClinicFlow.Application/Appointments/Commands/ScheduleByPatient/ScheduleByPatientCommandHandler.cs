using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;

public sealed class ScheduleByPatientCommandHandler(
    IPatientPenaltyRepository penaltyRepository,
    IPatientRepository patientRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IScheduleRepository scheduleRepository,
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ScheduleByPatientCommand, Guid>
{
    public async Task<Guid> Handle(
        ScheduleByPatientCommand request,
        CancellationToken cancellationToken
    )
    {
        var targetPatient =
            await patientRepository.GetByIdAsync(request.TargetPatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.TargetPatientId
            );

        var initiatorPatient =
            await patientRepository.GetByUserIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.InitiatorUserId
            );

        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(
                request.AppointmentTypeId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        var penalties = await penaltyRepository.GetByPatientIdAsync(
            request.TargetPatientId,
            cancellationToken
        );
        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var doctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(
            request.DoctorId,
            request.ScheduledDate.DayOfWeek,
            cancellationToken
        );
        var hasConflict = await appointmentRepository.HasConflictAsync(
            request.DoctorId,
            request.ScheduledDate,
            timeRange,
            cancellationToken
        );

        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            appointmentType,
            new PatientSchedulingArgs
            {
                TargetPatient = targetPatient,
                InitiatorPatient = initiatorPatient,
                DoctorId = request.DoctorId,
                ScheduledDate = request.ScheduledDate,
                TimeRange = timeRange,
            },
            new AppointmentSchedulingContext
            {
                Penalties = penalties,
                DoctorSchedule = doctorSchedule,
                HasConflict = hasConflict,
            }
        );

        await appointmentRepository.CreateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
