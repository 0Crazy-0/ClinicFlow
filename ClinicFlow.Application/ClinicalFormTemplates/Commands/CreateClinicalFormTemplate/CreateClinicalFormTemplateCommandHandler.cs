using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

public sealed class CreateClinicalFormTemplateCommandHandler(
    IClinicalFormTemplateRepository clinicalFormTemplateRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateClinicalFormTemplateCommand, Guid>
{
    public async Task<Guid> Handle(CreateClinicalFormTemplateCommand request, CancellationToken ct)
    {
        if (await clinicalFormTemplateRepository.ExistsByCodeAsync(request.Code, ct))
            throw new BusinessRuleValidationException(
                DomainErrors.ClinicalFormTemplate.CodeAlreadyExists
            );

        if (await clinicalFormTemplateRepository.ExistsByNameAsync(request.Name, ct))
            throw new BusinessRuleValidationException(
                DomainErrors.ClinicalFormTemplate.NameAlreadyExists
            );

        var template = ClinicalFormTemplate.Create(
            request.Code,
            request.Name,
            request.Description,
            request.JsonSchemaDefinition
        );

        await clinicalFormTemplateRepository.CreateAsync(template, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return template.Id;
    }
}
