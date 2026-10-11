using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Common.Models;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetMinorFamilyMemberAppointmentsByPatientId;

public sealed record GetMinorFamilyMemberAppointmentsByPatientIdQuery(
    Guid RequesterUserId,
    Guid PatientId,
    int PageNumber,
    int PageSize
) : IRequest<PaginatedList<MinorFamilyAppointmentDto>>;
