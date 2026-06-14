using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

public sealed class CreateClinicalFormTemplateCommandValidator
    : AbstractValidator<CreateClinicalFormTemplateCommand>
{
    public CreateClinicalFormTemplateCommandValidator(
        IJsonSchemaDefinitionValidator schemaDefinitionValidator
    )
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MaximumLength(100)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage(DomainErrors.Validation.ValueTooLong);

        RuleFor(x => x.JsonSchemaDefinition)
            .Must(json => schemaDefinitionValidator.IsValidSchema(json, out _))
            .WithMessage(DomainErrors.Validation.InvalidFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.JsonSchemaDefinition));
    }
}
