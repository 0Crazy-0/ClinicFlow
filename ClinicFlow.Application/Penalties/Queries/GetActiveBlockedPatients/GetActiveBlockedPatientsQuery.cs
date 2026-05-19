using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Penalties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveBlockedPatients;

public sealed record GetActiveBlockedPatientsQuery(int PageNumber, int PageSize)
    : IRequest<PaginatedList<PatientPenaltyDto>>;
