using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Services.Policies;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;

public sealed class UpdateClinicalFormTemplateCommandValidator
    : AbstractValidator<UpdateClinicalFormTemplateCommand>
{
    public UpdateClinicalFormTemplateCommandValidator(
        IJsonSchemaDefinitionValidator schemaDefinitionValidator
    )
    {
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
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
