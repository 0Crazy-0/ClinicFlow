using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Penalties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveWarnings;

public sealed record GetActiveWarningsQuery(int PageNumber, int PageSize)
    : IRequest<PaginatedList<PatientPenaltyDto>>;
