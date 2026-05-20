using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.DeactivateClinicalFormTemplate;

public sealed class DeactivateClinicalFormTemplateCommandHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeactivateClinicalFormTemplateCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        DeactivateClinicalFormTemplateCommand request,
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

        template.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
