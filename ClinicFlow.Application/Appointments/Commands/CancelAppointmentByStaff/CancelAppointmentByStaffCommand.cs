using ClinicFlow.Application.Appointments.Commands.Shared.Cancel;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;

public sealed record CancelAppointmentByStaffCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string Reason
) : IRequest, ICancelCommand;
