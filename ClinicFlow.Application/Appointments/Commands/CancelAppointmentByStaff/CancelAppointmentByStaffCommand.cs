using MediatR;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;

public record CancelAppointmentByStaffCommand(Guid AppointmentId, Guid InitiatorUserId, UserRole InitiatorRole, string Reason) : IRequest;
