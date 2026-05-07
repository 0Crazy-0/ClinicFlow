using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CleanExpiredDisplacedAppointments;

public sealed record CleanExpiredDisplacedAppointmentsCommand : IRequest;
