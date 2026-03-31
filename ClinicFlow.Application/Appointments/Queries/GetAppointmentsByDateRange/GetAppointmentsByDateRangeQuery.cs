using ClinicFlow.Application.Appointments.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;

public sealed record GetAppointmentsByDateRangeQuery(DateTime StartDate, DateTime EndDate)
    : IRequest<IEnumerable<AppointmentDto>>;
