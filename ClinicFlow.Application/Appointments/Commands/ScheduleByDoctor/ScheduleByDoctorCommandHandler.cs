using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByDoctor;

public sealed class ScheduleByDoctorCommandHandler(
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IScheduleRepository scheduleRepository,
    IAppointmentRepository appointmentRepository,
    IRegionalSchedulingService regionalSchedulingService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ScheduleByDoctorCommand, Guid>
{
    public async Task<Guid> Handle(ScheduleByDoctorCommand request, CancellationToken ct)
    {
        var initiatorDoctor =
            await doctorRepository.GetByUserIdAsync(request.InitiatorUserId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.InitiatorUserId
            );

        var targetPatient =
            await patientRepository.GetByIdAsync(request.TargetPatientId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.TargetPatientId
            );

        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(request.AppointmentTypeId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var doctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(
            initiatorDoctor.Id,
            request.ScheduledDate.DayOfWeek,
            ct
        );
        var hasConflict = await appointmentRepository.HasConflictAsync(
            initiatorDoctor.Id,
            request.ScheduledDate,
            timeRange,
            ct
        );

        var clearance = regionalSchedulingService.EnforceSchedulingRegulations(
            initiatorDoctor,
            targetPatient,
            appointmentType
        );

        var appointment = AppointmentSchedulingService.ScheduleByDoctor(
            appointmentType,
            new DoctorSchedulingArgs
            {
                InitiatorDoctor = initiatorDoctor,
                TargetPatient = targetPatient,
                ScheduledDate = request.ScheduledDate,
                TimeRange = timeRange,
                IsOverbook = request.IsOverbook,
                HasGuardianConsentVerified = request.HasGuardianConsentVerified,
            },
            new AppointmentSchedulingContext
            {
                Penalties = [],
                DoctorSchedule = doctorSchedule,
                HasConflict = hasConflict,
            },
            clearance
        );

        await appointmentRepository.CreateAsync(appointment, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return appointment.Id;
    }
}
