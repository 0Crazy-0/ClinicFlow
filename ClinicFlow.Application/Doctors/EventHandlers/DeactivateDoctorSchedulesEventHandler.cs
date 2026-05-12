using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.EventHandlers;

public sealed class DeactivateDoctorSchedulesEventHandler(IScheduleRepository scheduleRepository)
    : INotificationHandler<DomainEventNotification<DoctorSuspendedEvent>>
{
    public async Task Handle(
        DomainEventNotification<DoctorSuspendedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var doctorId = notification.DomainEvent.DoctorId;

        var activeSchedules = await scheduleRepository.GetActiveByDoctorIdAsync(
            doctorId,
            cancellationToken
        );

        foreach (var schedule in activeSchedules)
            schedule.Deactivate();
    }
}
