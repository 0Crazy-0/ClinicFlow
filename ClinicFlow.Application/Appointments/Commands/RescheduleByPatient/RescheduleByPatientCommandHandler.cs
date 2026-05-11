using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
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
    public async Task Handle(RescheduleByPatientCommand request, CancellationToken ct)
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var targetPatient =
            await patientRepository.GetByIdAsync(appointment.PatientId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                appointment.PatientId
            );

        var initiatorPatient =
            await patientRepository.GetByUserIdAsync(request.InitiatorUserId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.InitiatorUserId
            );

        var targetDoctor =
            await doctorRepository.GetByIdAsync(appointment.DoctorId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                appointment.DoctorId
            );

        var user =
            await userRepository.GetByIdAsync(request.InitiatorUserId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.InitiatorUserId
            );

        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(appointment.AppointmentTypeId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                appointment.AppointmentTypeId
            );

        var penalties = await penaltyRepository.GetByPatientIdAsync(appointment.PatientId, ct);

        var newTimeRange = TimeRange.Create(request.NewStartTime, request.NewEndTime);

        var doctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(
            appointment.DoctorId,
            request.NewDate.DayOfWeek,
            ct
        );

        var hasConflict = await appointmentRepository.HasConflictAsync(
            appointment.DoctorId,
            request.NewDate,
            newTimeRange,
            ct
        );

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
            },
            new AppointmentReschedulingContext
            {
                Penalties = penalties,
                DoctorSchedule = doctorSchedule,
                HasConflict = hasConflict,
            },
            clearance
        );

        await unitOfWork.SaveChangesAsync(ct);
    }
}
