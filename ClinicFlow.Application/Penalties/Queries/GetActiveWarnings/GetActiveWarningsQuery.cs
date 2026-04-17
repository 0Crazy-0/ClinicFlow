using ClinicFlow.Application.Penalties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveWarnings;

public sealed record GetActiveWarningsQuery : IRequest<IReadOnlyList<PatientPenaltyDto>>;
