using ClinicFlow.Application.Penalties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetPenaltiesByPatientId;

public sealed class GetPenaltiesByPatientIdQueryHandler(IPatientPenaltyRepository penaltyRepository)
    : IRequestHandler<GetPenaltiesByPatientIdQuery, IReadOnlyList<PatientPenaltyDto>>
{
    public async Task<IReadOnlyList<PatientPenaltyDto>> Handle(
        GetPenaltiesByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var penalties = await penaltyRepository.GetByPatientIdAsync(
            request.PatientId,
            cancellationToken
        );

        return
        [
            .. penalties.Select(p => new PatientPenaltyDto(
                p.Id,
                p.PatientId,
                p.AppointmentId,
                p.Type.ToString(),
                p.Reason,
                p.BlockedUntil,
                p.IsRemoved
            )),
        ];
    }
}
