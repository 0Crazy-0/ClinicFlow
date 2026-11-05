using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ClearPatientNotesByPatient;

public sealed record ClearPatientNotesByPatientCommand(Guid AppointmentId, Guid InitiatorUserId)
    : IRequest;
