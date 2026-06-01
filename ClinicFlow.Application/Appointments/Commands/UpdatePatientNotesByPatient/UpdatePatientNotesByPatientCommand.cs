using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByPatient;

public sealed record UpdatePatientNotesByPatientCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string? Notes
) : IRequest;
