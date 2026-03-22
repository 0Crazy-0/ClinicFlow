using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;

public record CancelAppointmentByPatientCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string? Reason
) : IRequest;
