using ClinicFlow.Application.Appointments.Commands.Shared.Cancel;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByDoctor;

public sealed record CancelAppointmentByDoctorCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string? Reason
) : IRequest, ICancelCommand;
