using ClinicFlow.Application.Appointments.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(Guid AppointmentId) : IRequest<AppointmentDto>;
