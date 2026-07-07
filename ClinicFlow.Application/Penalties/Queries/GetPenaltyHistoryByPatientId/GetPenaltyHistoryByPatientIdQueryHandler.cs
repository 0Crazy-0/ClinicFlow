using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Penalties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetPenaltyHistoryByPatientId;

public sealed class GetPenaltyHistoryByPatientIdQueryHandler(
    IPatientPenaltyRepository penaltyRepository
) : IRequestHandler<GetPenaltyHistoryByPatientIdQuery, PaginatedList<PatientPenaltyDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<PatientPenaltyDto>> Handle(
        GetPenaltyHistoryByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await penaltyRepository.GetHistoryByPatientIdPaginatedAsync(
            request.PatientId,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var dtos = items
            .Select(p => new PatientPenaltyDto(
                p.Id,
                p.PatientId,
                p.AppointmentId,
                p.Type.ToString(),
                p.Reason,
                p.BlockedUntil,
                p.IsRemoved
            ))
            .ToList();

        return new PaginatedList<PatientPenaltyDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}
