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
    /// <inheritdoc />
    public async Task Handle(
        UpdateClinicalFormTemplateCommand request,
        CancellationToken cancellationToken
    )
    {
        var template =
            await clinicalFormTemplateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(ClinicalFormTemplate),
                request.TemplateId
            );

        if (
            await clinicalFormTemplateRepository.ExistsByNameExcludingAsync(
                request.Name,
                request.TemplateId,
                cancellationToken
            )
        )
            throw new BusinessRuleValidationException(
                DomainErrors.ClinicalFormTemplate.NameAlreadyExists
            );

        template.UpdateDetails(request.Name, request.Description);
        template.UpdateSchema(request.JsonSchemaDefinition);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
