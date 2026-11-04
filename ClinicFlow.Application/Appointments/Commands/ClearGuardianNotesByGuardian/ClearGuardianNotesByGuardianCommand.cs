using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ClearGuardianNotesByGuardian;

public sealed record ClearGuardianNotesByGuardianCommand(Guid AppointmentId, Guid InitiatorUserId)
    : IRequest;
