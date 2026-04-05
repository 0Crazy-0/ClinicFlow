using ClinicFlow.Application.Appointments.Commands.Shared.Cancel;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;

public sealed record CancelAppointmentByPatientCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string? Reason
) : IRequest, ICancelCommand;
