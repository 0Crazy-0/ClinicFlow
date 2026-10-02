using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByGuardian;

public sealed record RescheduleByGuardianCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateOnly NewDate,
    TimeOnly NewStartTime,
    TimeOnly NewEndTime,
    string? NewGuardianNotes = null
) : IRequest;
