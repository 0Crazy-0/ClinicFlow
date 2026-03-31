using ClinicFlow.Application.Appointments.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentById;

public sealed record GetAppointmentByIdQuery(Guid AppointmentId) : IRequest<AppointmentDto>;
