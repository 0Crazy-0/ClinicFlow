using ClinicFlow.Application.ClinicalFormTemplates.Commands.Shared;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;

public class UpdateClinicalFormTemplateCommandValidator
    : ClinicalFormTemplateCommandValidatorBase<UpdateClinicalFormTemplateCommand>
{
    public UpdateClinicalFormTemplateCommandValidator(
        IJsonSchemaDefinitionValidator schemaDefinitionValidator
    )
        : base(schemaDefinitionValidator)
    {
        RuleFor(x => x.TemplateId).NotEmpty();
    }
}
