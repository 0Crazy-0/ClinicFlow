using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;

public sealed record GetAppointmentsByDateRangeQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    int PageNumber,
    int PageSize
) : IRequest<PaginatedList<AppointmentDto>>;
