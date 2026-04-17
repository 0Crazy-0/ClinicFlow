using ClinicFlow.Application.Penalties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetPenaltiesByPatientId;

public sealed record GetPenaltiesByPatientIdQuery(Guid PatientId)
    : IRequest<IReadOnlyList<PatientPenaltyDto>>;
