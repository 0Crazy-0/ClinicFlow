using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Schedules.EventHandlers;

public sealed class ScheduleDeactivatedEventHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IScheduleRepository scheduleRepository
) : INotificationHandler<DomainEventNotification<ScheduleDeactivatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<ScheduleDeactivatedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var domainEvent = notification.DomainEvent;
        var referenceDate = timeProvider.GetUtcNow().UtcDateTime.Date;

        var futureAppointments = await appointmentRepository.GetFutureScheduledByDoctorIdAsync(
            domainEvent.DoctorId,
            referenceDate,
            cancellationToken
        );

        var activeSchedule = await scheduleRepository.GetByDoctorAndDayAsync(
            domainEvent.DoctorId,
            domainEvent.DayOfWeek,
            cancellationToken
        );

        var appointmentsToReassign = futureAppointments
            .Where(a => a.ScheduledDate.DayOfWeek == domainEvent.DayOfWeek)
            .Where(a => activeSchedule is null || !activeSchedule.CoversTimeRange(a.TimeRange));

        foreach (var appointment in appointmentsToReassign)
            appointment.MarkAsRequiresReassignment();
    }
}
