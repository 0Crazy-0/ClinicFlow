using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;

/// <summary>
/// Orchestrates rescheduling an appointment initiated by a patient, including penalty checks, doctor slot availability, and conflict checks.
/// </summary>
public sealed class RescheduleByPatientCommandHandler(
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IScheduleRepository scheduleRepository,
    IPatientPenaltyRepository penaltyRepository,
    IUserRepository userRepository,
    IRegionalSchedulingService regionalSchedulingService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RescheduleByPatientCommand>
{
    /// <inheritdoc />
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
            await patientRepository.GetSelfPatientByUserIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.InitiatorUserId
            );

        var targetDoctor =
            await doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                appointment.DoctorId
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
                appointment.AppointmentTypeId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                appointment.AppointmentTypeId
            );

        var penalties = await penaltyRepository.GetHistoryByPatientIdAsync(
            appointment.PatientId,
            cancellationToken
        );

        var newTimeRange = TimeRange.Create(request.NewStartTime, request.NewEndTime);

        var doctorSchedule =
            await scheduleRepository.GetByDoctorAndDayAsync(
                appointment.DoctorId,
                request.NewDate.DayOfWeek,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Schedule),
                appointment.DoctorId
            );

        if (
            await appointmentRepository.HasConflictAsync(
                appointment.DoctorId,
                request.NewDate,
                newTimeRange,
                cancellationToken
            )
        )
        {
            throw new AppointmentConflictException(
                DomainErrors.Appointment.Conflict,
                appointment.DoctorId,
                request.NewDate.ToDateTime(newTimeRange.Start)
            );
        }

        var clearance = regionalSchedulingService.EnforceSchedulingRegulations(
            targetDoctor,
            targetPatient,
            appointmentType
        );

        AppointmentReschedulingService.RescheduleByPatient(
            appointment,
            new PatientReschedulingArgs
            {
                TargetPatient = targetPatient,
                InitiatorPatient = initiatorPatient,
                NewDate = request.NewDate,
                NewTimeRange = newTimeRange,
                IsInitiatorPhoneVerified = user.IsPhoneVerified,
                NewPatientNotes = request.NewPatientNotes,
            },
            new PatientReschedulingContext
            {
                Penalties = penalties,
                DoctorSchedule = doctorSchedule,
            },
            clearance
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
