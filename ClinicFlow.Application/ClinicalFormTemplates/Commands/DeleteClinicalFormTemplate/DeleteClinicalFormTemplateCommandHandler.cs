using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.DeleteClinicalFormTemplate;

public sealed class DeleteClinicalFormTemplateCommandHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteClinicalFormTemplateCommand>
{
    public async Task Handle(
        DeleteClinicalFormTemplateCommand request,
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

        template.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
