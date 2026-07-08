using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorIdAndDate;

public sealed record GetAppointmentsByDoctorIdAndDateQuery(
    Guid DoctorId,
    DateOnly Date,
    int PageNumber,
    int PageSize
) : IRequest<PaginatedList<AppointmentDto>>;
