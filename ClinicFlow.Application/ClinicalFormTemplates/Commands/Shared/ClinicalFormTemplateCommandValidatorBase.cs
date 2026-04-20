using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.Shared;

public abstract class ClinicalFormTemplateCommandValidatorBase<TCommand>
    : AbstractValidator<TCommand>
    where TCommand : IClinicalFormTemplateCommand
{
    protected ClinicalFormTemplateCommandValidatorBase(
        IJsonSchemaDefinitionValidator schemaDefinitionValidator
    )
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.JsonSchemaDefinition)
            .Must(json => schemaDefinitionValidator.IsValidSchema(json, out _))
            .WithMessage(DomainErrors.Validation.InvalidFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.JsonSchemaDefinition));
    }
}
