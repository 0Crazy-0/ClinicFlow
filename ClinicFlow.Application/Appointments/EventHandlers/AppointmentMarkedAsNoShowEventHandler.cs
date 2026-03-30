using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.EventHandlers;

public class AppointmentMarkedAsNoShowEventHandler(
    TimeProvider timeProvider,
    IPatientPenaltyRepository patientPenaltyRepository
) : INotificationHandler<DomainEventNotification<AppointmentMarkedAsNoShowEvent>>
{
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

        await patientPenaltyRepository.AddRangeAsync(newPenalties, cancellationToken);
    }
}
