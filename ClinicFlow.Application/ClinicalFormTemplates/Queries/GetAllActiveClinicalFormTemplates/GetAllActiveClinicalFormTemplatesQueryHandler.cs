using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetAllActiveClinicalFormTemplates;

public sealed class GetAllActiveClinicalFormTemplatesQueryHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository
) : IRequestHandler<GetAllActiveClinicalFormTemplatesQuery, IReadOnlyList<ClinicalFormTemplateDto>>
{
    public async Task<IReadOnlyList<ClinicalFormTemplateDto>> Handle(
        GetAllActiveClinicalFormTemplatesQuery request,
        CancellationToken cancellationToken
    )
    {
        var templates = await clinicalFormTemplateRepository.GetAllActiveAsync(cancellationToken);

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
