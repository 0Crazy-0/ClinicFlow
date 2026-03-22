using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;

public record CancelAppointmentByStaffCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    UserRole InitiatorRole,
    string Reason
) : IRequest;
