using ClinicFlow.Application.Penalties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveBlocksByPatientId;

public sealed class GetActiveBlocksByPatientIdQueryHandler(
    TimeProvider timeProvider,
    IPatientPenaltyRepository penaltyRepository
) : IRequestHandler<GetActiveBlocksByPatientIdQuery, IReadOnlyList<PatientPenaltyDto>>
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PatientPenaltyDto>> Handle(
        GetActiveBlocksByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var items = await penaltyRepository.GetActiveBlocksByPatientIdAsync(
            request.PatientId,
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime),
            cancellationToken
        );

        return
        [
            .. items.Select(p => new PatientPenaltyDto(
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
