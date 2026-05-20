using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.EventHandlers;

public sealed class AppointmentMarkedAsNoShowEventHandler(
    TimeProvider timeProvider,
    IPatientPenaltyRepository patientPenaltyRepository,
    IUnitOfWork unitOfWork
) : INotificationHandler<DomainEventNotification<AppointmentMarkedAsNoShowEvent>>
{
    /// <inheritdoc />
    public async Task Handle(
        DomainEventNotification<AppointmentMarkedAsNoShowEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var appointment = notification.DomainEvent.Appointment;

        var existingPenalties = await patientPenaltyRepository.GetByPatientIdAsync(
            appointment.PatientId,
            cancellationToken
        );

        var newPenalties = PatientPenaltyService.ApplyPenalty(
            appointment.PatientId,
            existingPenalties,
            appointment.Id,
            PenaltyReasons.NoShow,
            timeProvider.GetUtcNow().UtcDateTime
        );

        await patientPenaltyRepository.CreateRangeAsync(newPenalties, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
