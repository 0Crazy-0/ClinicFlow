using ClinicFlow.Application.Penalties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveWarnings;

public sealed class GetActiveWarningsQueryHandler(IPatientPenaltyRepository penaltyRepository)
    : IRequestHandler<GetActiveWarningsQuery, IReadOnlyList<PatientPenaltyDto>>
{
    public async Task<IReadOnlyList<PatientPenaltyDto>> Handle(
        GetActiveWarningsQuery request,
        CancellationToken cancellationToken
    )
    {
        var penalties = await penaltyRepository.GetActiveWarningsAsync(cancellationToken);

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
