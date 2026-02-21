using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId, Guid InitiatorUserId, bool IsAuthorizedFamilyMember, string? Reason) : IRequest;
