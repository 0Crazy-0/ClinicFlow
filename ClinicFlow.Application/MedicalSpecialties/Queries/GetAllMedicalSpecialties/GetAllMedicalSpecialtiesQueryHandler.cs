using ClinicFlow.Application.MedicalSpecialties.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Queries.GetAllMedicalSpecialties;

public sealed class GetAllMedicalSpecialtiesQueryHandler(
    IMedicalSpecialtyRepository medicalSpecialtyRepository
) : IRequestHandler<GetAllMedicalSpecialtiesQuery, IReadOnlyList<MedicalSpecialtyDto>>
{
    public async Task<IReadOnlyList<MedicalSpecialtyDto>> Handle(
        GetAllMedicalSpecialtiesQuery request,
        CancellationToken ct
    )
    {
        var specialties = await medicalSpecialtyRepository.GetAllIncludingDeletedAsync(ct);

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
