using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetOwnAppointmentsByPatientId;

public sealed record GetOwnAppointmentsByPatientIdQuery(
    Guid RequesterUserId,
    Guid PatientId,
    int PageNumber,
    int PageSize
) : IRequest<PaginatedList<PatientAppointmentDto>>;
