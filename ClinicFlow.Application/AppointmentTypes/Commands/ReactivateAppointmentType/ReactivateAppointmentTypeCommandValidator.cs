using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.ReactivateAppointmentType;

public class ReactivateAppointmentTypeCommandValidator
    : AbstractValidator<ReactivateAppointmentTypeCommand>
{
    public ReactivateAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
