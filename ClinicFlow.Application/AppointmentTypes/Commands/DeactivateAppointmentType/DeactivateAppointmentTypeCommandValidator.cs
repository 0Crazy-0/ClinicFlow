using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.DeactivateAppointmentType;

public sealed class DeactivateAppointmentTypeCommandValidator
    : AbstractValidator<DeactivateAppointmentTypeCommand>
{
    public DeactivateAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
