using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

public sealed class CreateClinicalFormTemplateCommandHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateClinicalFormTemplateCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateClinicalFormTemplateCommand request,
        CancellationToken cancellationToken
    )
    {
        var template = ClinicalFormTemplate.Create(
            request.Code,
            request.Name,
            request.Description,
            request.JsonSchemaDefinition
        );

        await clinicalFormTemplateRepository.CreateAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
