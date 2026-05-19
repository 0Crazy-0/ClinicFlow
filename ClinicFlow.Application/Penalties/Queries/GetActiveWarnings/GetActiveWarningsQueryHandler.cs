using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Penalties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveWarnings;

public sealed class GetActiveWarningsQueryHandler(IPatientPenaltyRepository penaltyRepository)
    : IRequestHandler<GetActiveWarningsQuery, PaginatedList<PatientPenaltyDto>>
{
    public async Task<PaginatedList<PatientPenaltyDto>> Handle(
        GetActiveWarningsQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await penaltyRepository.GetActiveWarningsPaginatedAsync(
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
