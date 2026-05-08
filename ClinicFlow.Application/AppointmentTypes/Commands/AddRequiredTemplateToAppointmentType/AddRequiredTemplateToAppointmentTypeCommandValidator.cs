using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.AddRequiredTemplateToAppointmentType;

public class AddRequiredTemplateToAppointmentTypeCommandValidator
    : AbstractValidator<AddRequiredTemplateToAppointmentTypeCommand>
{
    public AddRequiredTemplateToAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
