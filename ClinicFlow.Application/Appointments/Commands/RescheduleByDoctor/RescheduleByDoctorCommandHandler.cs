using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByDoctor;

public sealed class RescheduleByDoctorCommandHandler(
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IScheduleRepository scheduleRepository,
    IRegionalSchedulingService regionalSchedulingService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RescheduleByDoctorCommand>
{
    /// <inheritdoc />
    public async Task Handle(RescheduleByDoctorCommand request, CancellationToken cancellationToken)
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var initiatorDoctor =
            await doctorRepository.GetByUserIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.InitiatorUserId
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
            !request.IsOverbook
            && await appointmentRepository.HasConflictAsync(
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
            initiatorDoctor,
            targetPatient,
            appointmentType
        );

        AppointmentReschedulingService.RescheduleByDoctor(
            appointment,
            new DoctorReschedulingArgs
            {
                InitiatorDoctor = initiatorDoctor,
                NewDate = request.NewDate,
                NewTimeRange = newTimeRange,
                IsOverbook = request.IsOverbook,
            },
            doctorSchedule,
            clearance
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
