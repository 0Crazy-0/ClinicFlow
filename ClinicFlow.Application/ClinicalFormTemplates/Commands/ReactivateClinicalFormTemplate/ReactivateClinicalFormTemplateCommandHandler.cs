using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.ReactivateClinicalFormTemplate;

public sealed class ReactivateClinicalFormTemplateCommandHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReactivateClinicalFormTemplateCommand>
{
    public async Task Handle(ReactivateClinicalFormTemplateCommand request, CancellationToken ct)
    {
        var template =
            await clinicalFormTemplateRepository.GetByIdIncludingDeletedAsync(
                request.TemplateId,
                ct
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(ClinicalFormTemplate),
                request.TemplateId
            );

        if (await clinicalFormTemplateRepository.ExistsByCodeAsync(template.Code, ct))
            throw new BusinessRuleValidationException(
                DomainErrors.ClinicalFormTemplate.CodeAlreadyExists
            );

        if (await clinicalFormTemplateRepository.ExistsByNameAsync(template.Name, ct))
            throw new BusinessRuleValidationException(
                DomainErrors.ClinicalFormTemplate.NameAlreadyExists
            );

        template.Reactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
