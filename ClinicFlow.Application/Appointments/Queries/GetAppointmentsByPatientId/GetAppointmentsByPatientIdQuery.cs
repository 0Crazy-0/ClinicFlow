using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;

public sealed record GetAppointmentsByPatientIdQuery(Guid PatientId, int PageNumber, int PageSize)
    : IRequest<PaginatedList<AppointmentDto>>;
