using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;

public sealed record GetAppointmentsByDoctorIdQuery(
    Guid DoctorId,
    DateOnly Date,
    int PageNumber,
    int PageSize
) : IRequest<PaginatedList<AppointmentDto>>;
