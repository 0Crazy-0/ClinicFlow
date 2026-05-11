using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetAllClinicalFormTemplates;

public sealed class GetAllClinicalFormTemplatesQueryHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository
) : IRequestHandler<GetAllClinicalFormTemplatesQuery, IReadOnlyList<ClinicalFormTemplateDto>>
{
    public async Task<IReadOnlyList<ClinicalFormTemplateDto>> Handle(
        GetAllClinicalFormTemplatesQuery request,
        CancellationToken cancellationToken
    )
    {
        var templates = await clinicalFormTemplateRepository.GetAllIncludingDeletedAsync(
            cancellationToken
        );

        return
        [
            .. templates.Select(template => new ClinicalFormTemplateDto(
                template.Id,
                template.Code,
                template.Name,
                template.Description,
                template.JsonSchemaDefinition,
                template.IsDeleted
            )),
        ];
    }
}
