using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Events.Schedules;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Schedules.EventHandlers;

public sealed class ScheduleDeactivatedEventHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork
) : INotificationHandler<DomainEventNotification<ScheduleDeactivatedEvent>>
{
    /// <inheritdoc />
    public async Task Handle(
        DomainEventNotification<ScheduleDeactivatedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var domainEvent = notification.DomainEvent;
        var referenceDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var futureAppointments = await appointmentRepository.GetFutureScheduledByDoctorIdAsync(
            domainEvent.DoctorId,
            referenceDate,
            cancellationToken
        );

        var activeSchedule = await scheduleRepository.GetActiveByDoctorAndDayAsync(
            domainEvent.DoctorId,
            domainEvent.DayOfWeek,
            cancellationToken
        );

        var appointmentsToReassign = futureAppointments
            .Where(a => a.ScheduledDate.DayOfWeek == domainEvent.DayOfWeek)
            .Where(a => activeSchedule is null || !activeSchedule.CoversTimeRange(a.TimeRange));

        foreach (var appointment in appointmentsToReassign)
            appointment.MarkAsRequiresReassignment();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
