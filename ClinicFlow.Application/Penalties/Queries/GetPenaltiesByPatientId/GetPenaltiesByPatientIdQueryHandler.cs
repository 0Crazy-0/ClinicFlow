using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Penalties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetPenaltiesByPatientId;

public sealed class GetPenaltiesByPatientIdQueryHandler(IPatientPenaltyRepository penaltyRepository)
    : IRequestHandler<GetPenaltiesByPatientIdQuery, PaginatedList<PatientPenaltyDto>>
{
    public async Task<PaginatedList<PatientPenaltyDto>> Handle(
        GetPenaltiesByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await penaltyRepository.GetByPatientIdPaginatedAsync(
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
