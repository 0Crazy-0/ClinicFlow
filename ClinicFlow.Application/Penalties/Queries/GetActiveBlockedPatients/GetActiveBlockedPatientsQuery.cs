using ClinicFlow.Application.Penalties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveBlockedPatients;

public sealed record GetActiveBlockedPatientsQuery : IRequest<IReadOnlyList<PatientPenaltyDto>>;
