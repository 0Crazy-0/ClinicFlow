using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateByCode;

public sealed class GetClinicalFormTemplateByCodeQueryHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository
) : IRequestHandler<GetClinicalFormTemplateByCodeQuery, ClinicalFormTemplateDto>
{
    /// <inheritdoc />
    public async Task<ClinicalFormTemplateDto> Handle(
        GetClinicalFormTemplateByCodeQuery request,
        CancellationToken cancellationToken
    )
    {
        var template =
            await clinicalFormTemplateRepository.GetByCodeAsync(request.Code, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(ClinicalFormTemplate),
                request.Code
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
