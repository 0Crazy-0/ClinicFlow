using ClinicFlow.Application.ClinicalFormTemplates.Commands.Shared;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

public class CreateClinicalFormTemplateCommandValidator
    : ClinicalFormTemplateCommandValidatorBase<CreateClinicalFormTemplateCommand>
{
    public CreateClinicalFormTemplateCommandValidator(
        IJsonSchemaDefinitionValidator schemaDefinitionValidator
    )
        : base(schemaDefinitionValidator)
    {
        RuleFor(x => x.Code).NotEmpty();
    }
}
