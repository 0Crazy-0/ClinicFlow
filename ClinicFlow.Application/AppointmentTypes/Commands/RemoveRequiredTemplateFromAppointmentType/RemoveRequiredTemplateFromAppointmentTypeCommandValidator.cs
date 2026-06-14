using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RemoveRequiredTemplateFromAppointmentType;

public sealed class RemoveRequiredTemplateFromAppointmentTypeCommandValidator
    : AbstractValidator<RemoveRequiredTemplateFromAppointmentTypeCommand>
{
    public RemoveRequiredTemplateFromAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
