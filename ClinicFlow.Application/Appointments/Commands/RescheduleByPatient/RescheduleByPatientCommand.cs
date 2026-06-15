using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;

public sealed record RescheduleByPatientCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateTime NewDate,
    TimeOnly NewStartTime,
    TimeOnly NewEndTime,
    string? NewPatientNotes = null
) : IRequest;
