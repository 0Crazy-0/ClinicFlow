using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;

public record CancelAppointmentByStaffCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string Reason
) : IRequest;
