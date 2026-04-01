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

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;

public sealed class RescheduleByStaffCommandHandler(
    IAppointmentRepository appointmentRepository,
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RescheduleByStaffCommand>
{
    public async Task Handle(RescheduleByStaffCommand request, CancellationToken cancellationToken)
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
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

        AppointmentReschedulingService.RescheduleByStaff(
            appointment,
            new StaffReschedulingArgs
            {
                InitiatorUserId = request.InitiatorUserId,
                NewDate = request.NewDate,
                NewTimeRange = newTimeRange,
                IsOverbook = request.IsOverbook,
            },
            new AppointmentReschedulingContext
            {
                Penalties = [],
                DoctorSchedule = doctorSchedule,
                HasConflict = hasConflict,
            }
        );

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
