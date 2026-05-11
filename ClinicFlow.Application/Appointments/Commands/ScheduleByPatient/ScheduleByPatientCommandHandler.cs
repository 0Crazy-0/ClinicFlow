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

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;

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
    public async Task<Guid> Handle(ScheduleByPatientCommand request, CancellationToken ct)
    {
        var targetPatient =
            await patientRepository.GetByIdAsync(request.TargetPatientId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.TargetPatientId
            );

        var initiatorPatient =
            await patientRepository.GetByUserIdAsync(request.InitiatorUserId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.InitiatorUserId
            );

        var targetDoctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.DoctorId
            );

        var user =
            await userRepository.GetByIdAsync(request.InitiatorUserId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.InitiatorUserId
            );

        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(request.AppointmentTypeId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        var penalties = await penaltyRepository.GetByPatientIdAsync(request.TargetPatientId, ct);
        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var doctorSchedule = await scheduleRepository.GetByDoctorAndDayAsync(
            request.DoctorId,
            request.ScheduledDate.DayOfWeek,
            ct
        );
        var hasConflict = await appointmentRepository.HasConflictAsync(
            request.DoctorId,
            request.ScheduledDate,
            timeRange,
            ct
        );

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
            },
            new AppointmentSchedulingContext
            {
                Penalties = penalties,
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
