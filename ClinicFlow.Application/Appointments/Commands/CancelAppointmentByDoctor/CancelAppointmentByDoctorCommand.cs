using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByDoctor;

public record CancelAppointmentByDoctorCommand(Guid AppointmentId, Guid InitiatorUserId, string? Reason) : IRequest;
