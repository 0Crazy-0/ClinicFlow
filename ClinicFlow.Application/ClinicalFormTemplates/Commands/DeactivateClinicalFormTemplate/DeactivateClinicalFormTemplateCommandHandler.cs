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
    public async Task Handle(DeactivateClinicalFormTemplateCommand request, CancellationToken ct)
    {
        var template =
            await clinicalFormTemplateRepository.GetByIdAsync(request.TemplateId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(ClinicalFormTemplate),
                request.TemplateId
            );

        template.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
