using ClinicFlow.Application.MedicalSpecialties.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Queries.GetMedicalSpecialtyById;

public sealed class GetMedicalSpecialtyByIdQueryHandler(
    IMedicalSpecialtyRepository medicalSpecialtyRepository
) : IRequestHandler<GetMedicalSpecialtyByIdQuery, MedicalSpecialtyDto>
{
    public async Task<MedicalSpecialtyDto> Handle(
        GetMedicalSpecialtyByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var specialty =
            await medicalSpecialtyRepository.GetByIdAsync(
                request.MedicalSpecialtyId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalSpecialty),
                request.MedicalSpecialtyId
            );

        return new MedicalSpecialtyDto(
            specialty.Id,
            specialty.Name,
            specialty.Description,
            specialty.TypicalDurationMinutes,
            specialty.CancellationPolicy.Hours,
            specialty.IsDeleted
        );
    }
}
