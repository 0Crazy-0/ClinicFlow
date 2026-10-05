using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByGuardian;

public sealed class ScheduleByGuardianCommandHandler(
    IPatientPenaltyRepository penaltyRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IScheduleRepository scheduleRepository,
    IAppointmentRepository appointmentRepository,
    IUserRepository userRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    IRegionalSchedulingService regionalSchedulingService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ScheduleByGuardianCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        ScheduleByGuardianCommand request,
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

        var penalties = await penaltyRepository.GetHistoryByPatientIdAsync(
            request.TargetPatientId,
            cancellationToken
        );

        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var doctorSchedule =
            await scheduleRepository.GetActiveByDoctorAndDayAsync(
                request.DoctorId,
                request.ScheduledDate.DayOfWeek,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Schedule),
                request.DoctorId
            );

        return await unitOfWork.ExecuteWithLockAsync(
            request.TargetPatientId,
            async cancellationToken =>
            {
                if (
                    await appointmentRepository.HasActiveAppointmentForPatientAsync(
                        request.TargetPatientId,
                        appointmentType.Id,
                        cancellationToken: cancellationToken
                    )
                )
                {
                    throw new AppointmentDuplicateException(
                        DomainErrors.Appointment.Duplicate,
                        request.TargetPatientId,
                        appointmentType.Id
                    );
                }

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

                var initiatorMembership =
                    await familyMembershipRepository.GetActiveMembershipAsync(
                        request.InitiatorUserId,
                        request.TargetPatientId,
                        cancellationToken
                    )
                    ?? throw new PatientAccessUnauthorizedException(
                        DomainErrors.Patient.UnauthorizedAccess
                    );

                var appointment = AppointmentSchedulingService.ScheduleByGuardian(
                    appointmentType,
                    new GuardianSchedulingArgs
                    {
                        TargetPatient = targetPatient,
                        InitiatorUserId = request.InitiatorUserId,
                        DoctorId = request.DoctorId,
                        ScheduledDate = request.ScheduledDate,
                        TimeRange = timeRange,
                        IsInitiatorPhoneVerified = user.IsPhoneVerified,
                        GuardianNotes = request.GuardianNotes,
                    },
                    new PatientSchedulingContext
                    {
                        Penalties = penalties,
                        DoctorSchedule = doctorSchedule,
                        InitiatorMembership = initiatorMembership,
                        RequestedCategory = appointmentType.Category,
                    },
                    clearance
                );

                await appointmentRepository.CreateAsync(appointment, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return appointment.Id;
            },
            cancellationToken
        );
    }
}
