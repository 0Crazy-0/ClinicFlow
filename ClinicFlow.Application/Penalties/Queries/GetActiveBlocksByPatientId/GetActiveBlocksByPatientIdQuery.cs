using ClinicFlow.Application.Penalties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveBlocksByPatientId;

public sealed record GetActiveBlocksByPatientIdQuery(Guid PatientId)
    : IRequest<IReadOnlyList<PatientPenaltyDto>>;
