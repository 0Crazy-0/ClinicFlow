using ClinicFlow.Application.MedicalSpecialties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Queries.GetActiveMedicalSpecialties;

public sealed class GetActiveMedicalSpecialtiesQueryHandler(
    IMedicalSpecialtyRepository medicalSpecialtyRepository
) : IRequestHandler<GetActiveMedicalSpecialtiesQuery, IReadOnlyList<MedicalSpecialtyDto>>
{
    public async Task<IReadOnlyList<MedicalSpecialtyDto>> Handle(
        GetActiveMedicalSpecialtiesQuery request,
        CancellationToken ct
    )
    {
        var specialties = await medicalSpecialtyRepository.GetAllActiveAsync(ct);

        return
        [
            .. specialties.Select(specialty => new MedicalSpecialtyDto(
                specialty.Id,
                specialty.Name,
                specialty.Description,
                specialty.TypicalDurationMinutes,
                specialty.MinCancellationHours,
                specialty.IsDeleted
            )),
        ];
    }
}
