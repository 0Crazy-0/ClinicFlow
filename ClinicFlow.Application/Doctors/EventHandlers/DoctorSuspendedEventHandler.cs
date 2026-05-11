using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.EventHandlers;

public sealed class DoctorSuspendedEventHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository
) : INotificationHandler<DomainEventNotification<DoctorSuspendedEvent>>
{
    public async Task Handle(
        DomainEventNotification<DoctorSuspendedEvent> notification,
        CancellationToken ct
    )
    {
        var doctorId = notification.DomainEvent.DoctorId;
        var referenceDate = timeProvider.GetUtcNow().UtcDateTime.Date;
        var futureAppointments = await appointmentRepository.GetFutureScheduledByDoctorIdAsync(
            doctorId,
            referenceDate,
            ct
        );

        foreach (var appointment in futureAppointments)
            appointment.MarkAsRequiresReassignment();
    }
}
