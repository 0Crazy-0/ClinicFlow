using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShow;

public record MarkAppointmentAsNoShowCommand(Guid AppointmentId, Guid InitiatorUserId) : IRequest;
