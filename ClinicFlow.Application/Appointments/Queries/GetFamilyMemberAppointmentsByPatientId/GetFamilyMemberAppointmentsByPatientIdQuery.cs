using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetFamilyMemberAppointmentsByPatientId;

public sealed record GetFamilyMemberAppointmentsByPatientIdQuery(
    Guid RequesterUserId,
    Guid PatientId,
    int PageNumber,
    int PageSize
) : IRequest<PaginatedList<AppointmentDto>>;
