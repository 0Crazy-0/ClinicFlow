using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateById;

public sealed class GetClinicalFormTemplateByIdQueryHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository
) : IRequestHandler<GetClinicalFormTemplateByIdQuery, ClinicalFormTemplateDto>
{
    public async Task<ClinicalFormTemplateDto> Handle(
        GetClinicalFormTemplateByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var template =
            await clinicalFormTemplateRepository.GetByIdAsync(
                request.ClinicalFormTemplateId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(ClinicalFormTemplate),
                request.ClinicalFormTemplateId
            );

        return new ClinicalFormTemplateDto(
            template.Id,
            template.Code,
            template.Name,
            template.Description,
            template.JsonSchemaDefinition,
            template.IsDeleted
        );
    }
}
