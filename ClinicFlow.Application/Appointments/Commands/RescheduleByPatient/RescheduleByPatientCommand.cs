using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;

public sealed record RescheduleByPatientCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateTime NewDate,
    TimeSpan NewStartTime,
    TimeSpan NewEndTime,
    string? NewPatientNotes = null
) : IRequest;
