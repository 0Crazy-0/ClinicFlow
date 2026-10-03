using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdateGuardianNotesByGuardian;

public sealed record UpdateGuardianNotesByGuardianCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string? Notes
) : IRequest;
