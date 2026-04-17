using ClinicFlow.Application.Penalties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveBlockedPatients;

public sealed class GetActiveBlockedPatientsQueryHandler(
    TimeProvider timeProvider,
    IPatientPenaltyRepository penaltyRepository
) : IRequestHandler<GetActiveBlockedPatientsQuery, IReadOnlyList<PatientPenaltyDto>>
{
    public async Task<IReadOnlyList<PatientPenaltyDto>> Handle(
        GetActiveBlockedPatientsQuery request,
        CancellationToken cancellationToken
    )
    {
        var penalties = await penaltyRepository.GetActiveBlocksAsync(
            timeProvider.GetUtcNow().UtcDateTime,
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
