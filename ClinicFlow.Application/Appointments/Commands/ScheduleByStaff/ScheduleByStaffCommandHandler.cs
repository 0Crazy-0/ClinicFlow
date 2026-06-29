using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByStaff;

public sealed class ScheduleByStaffCommandHandler(
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IScheduleRepository scheduleRepository,
    IAppointmentRepository appointmentRepository,
    IRegionalSchedulingService regionalSchedulingService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ScheduleByStaffCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        ScheduleByStaffCommand request,
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

        var targetDoctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.DoctorId
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

        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var doctorSchedule =
            await scheduleRepository.GetByDoctorAndDayAsync(
                request.DoctorId,
                request.ScheduledDate.DayOfWeek,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Schedule),
                request.DoctorId
            );

        if (
            !request.IsOverbook
            && await appointmentRepository.HasConflictAsync(
                request.DoctorId,
                request.ScheduledDate,
                timeRange,
                cancellationToken
            )
        )
        {
            throw new AppointmentConflictException(
                DomainErrors.Appointment.Conflict,
                request.DoctorId,
                request.ScheduledDate.ToDateTime(timeRange.Start)
            );
        }

        var clearance = regionalSchedulingService.EnforceSchedulingRegulations(
            targetDoctor,
            targetPatient,
            appointmentType
        );

        var appointment = AppointmentSchedulingService.ScheduleByStaff(
            appointmentType,
            new StaffSchedulingArgs
            {
                InitiatorUserId = request.InitiatorUserId,
                TargetPatient = targetPatient,
                DoctorId = request.DoctorId,
                ScheduledDate = request.ScheduledDate,
                TimeRange = timeRange,
                HasGuardianConsentVerified = request.HasGuardianConsentVerified,
                IsOverbook = request.IsOverbook,
            },
            doctorSchedule,
            clearance
        );

        await appointmentRepository.CreateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
