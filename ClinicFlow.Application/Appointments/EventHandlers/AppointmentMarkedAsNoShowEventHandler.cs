using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.EventHandlers;

public sealed class AppointmentMarkedAsNoShowEventHandler(
    TimeProvider timeProvider,
    IPatientPenaltyRepository patientPenaltyRepository
) : INotificationHandler<DomainEventNotification<AppointmentMarkedAsNoShowEvent>>
{
    public async Task Handle(
        DomainEventNotification<AppointmentMarkedAsNoShowEvent> notification,
        CancellationToken ct
    )
    {
        var appointment = notification.DomainEvent.Appointment;

        var existingPenalties = await patientPenaltyRepository.GetByPatientIdAsync(
            appointment.PatientId,
            ct
        );

        var newPenalties = PatientPenaltyService.ApplyPenalty(
            appointment.PatientId,
            existingPenalties,
            appointment.Id,
            PenaltyReasons.NoShow,
            timeProvider.GetUtcNow().UtcDateTime
        );

        await patientPenaltyRepository.AddRangeAsync(newPenalties, ct);
    }
}
