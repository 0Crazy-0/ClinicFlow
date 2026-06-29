using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;

/// <summary>
/// Orchestrates scheduling a new appointment initiated by a patient, including patient relationship validation, penalty checks, and slot availability checks.
/// </summary>
public sealed class ScheduleByPatientCommandHandler(
    IPatientPenaltyRepository penaltyRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IScheduleRepository scheduleRepository,
    IAppointmentRepository appointmentRepository,
    IUserRepository userRepository,
    IRegionalSchedulingService regionalSchedulingService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ScheduleByPatientCommand, Guid>
{
    /// <inheritdoc />
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

        var targetDoctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.DoctorId
            );

        var user =
            await userRepository.GetByIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
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

        if (
            await appointmentRepository.HasConflictAsync(
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

        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            appointmentType,
            new PatientSchedulingArgs
            {
                TargetPatient = targetPatient,
                InitiatorPatient = initiatorPatient,
                DoctorId = request.DoctorId,
                ScheduledDate = request.ScheduledDate,
                TimeRange = timeRange,
                IsInitiatorPhoneVerified = user.IsPhoneVerified,
                PatientNotes = request.PatientNotes,
            },
            new AppointmentSchedulingContext
            {
                Penalties = penalties,
                DoctorSchedule = doctorSchedule,
            },
            clearance
        );

        await appointmentRepository.CreateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
