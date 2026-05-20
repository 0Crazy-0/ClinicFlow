using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Doctors.EventHandlers;

public sealed class DeactivateDoctorSchedulesEventHandler(
    IScheduleRepository scheduleRepository,
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

        var activeSchedules = await scheduleRepository.GetActiveByDoctorIdAsync(
            doctorId,
            cancellationToken
        );

        foreach (var schedule in activeSchedules)
            schedule.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
