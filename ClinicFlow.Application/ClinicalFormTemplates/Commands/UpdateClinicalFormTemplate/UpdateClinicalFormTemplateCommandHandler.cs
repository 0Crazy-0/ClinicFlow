using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;

public sealed class UpdateClinicalFormTemplateCommandHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateClinicalFormTemplateCommand>
{
    public async Task Handle(UpdateClinicalFormTemplateCommand request, CancellationToken ct)
    {
        var template =
            await clinicalFormTemplateRepository.GetByIdAsync(request.TemplateId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(ClinicalFormTemplate),
                request.TemplateId
            );

        if (
            await clinicalFormTemplateRepository.ExistsByNameExcludingAsync(
                request.Name,
                request.TemplateId,
                ct
            )
        )
            throw new BusinessRuleValidationException(
                DomainErrors.ClinicalFormTemplate.NameAlreadyExists
            );

        template.UpdateDetails(request.Name, request.Description);
        template.UpdateSchema(request.JsonSchemaDefinition);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
