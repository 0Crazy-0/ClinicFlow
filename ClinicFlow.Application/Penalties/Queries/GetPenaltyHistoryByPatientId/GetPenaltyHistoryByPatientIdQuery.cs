using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Penalties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetPenaltyHistoryByPatientId;

public sealed record GetPenaltyHistoryByPatientIdQuery(Guid PatientId, int PageNumber, int PageSize)
    : IRequest<PaginatedList<PatientPenaltyDto>>;
