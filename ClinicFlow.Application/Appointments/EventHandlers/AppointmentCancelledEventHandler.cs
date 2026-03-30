using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.EventHandlers;

public class AppointmentCancelledEventHandler(
    TimeProvider timeProvider,
    IPatientPenaltyRepository patientPenaltyRepository
) : INotificationHandler<DomainEventNotification<AppointmentCancelledEvent>>
{
    public async Task Handle(
        DomainEventNotification<AppointmentCancelledEvent> notification,
        CancellationToken cancellationToken
    )
    {
        var appointment = notification.DomainEvent.Appointment;

        if (appointment.Status is AppointmentStatus.LateCancellation)
        {
            var existingPenalties = await patientPenaltyRepository.GetByPatientIdAsync(
                appointment.PatientId,
                cancellationToken
            );
            var newPenalties = PatientPenaltyService.ApplyPenalty(
                appointment.PatientId,
                existingPenalties,
                appointment.Id,
                PenaltyReasons.LateCancellation,
                timeProvider.GetUtcNow().UtcDateTime
            );

            await patientPenaltyRepository.AddRangeAsync(newPenalties, cancellationToken);
        }
    }
}
