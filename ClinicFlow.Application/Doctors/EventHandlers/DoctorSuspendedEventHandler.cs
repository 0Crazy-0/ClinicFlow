using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.EventHandlers;

public sealed class DoctorSuspendedEventHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : INotificationHandler<DomainEventNotification<DoctorSuspendedEvent>>
{
    /// <inheritdoc />
    public async Task Handle(
        DomainEventNotification<DoctorSuspendedEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var doctorId = notification.DomainEvent.DoctorId;
        var referenceDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var futureAppointments = await appointmentRepository.GetFutureScheduledByDoctorIdAsync(
            doctorId,
            referenceDate,
            cancellationToken
        );

        foreach (var appointment in futureAppointments)
            appointment.MarkAsRequiresReassignment();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
