using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RemoveAllowedSpecialtyFromAppointmentType;

public class RemoveAllowedSpecialtyFromAppointmentTypeCommandValidator
    : AbstractValidator<RemoveAllowedSpecialtyFromAppointmentTypeCommand>
{
    public RemoveAllowedSpecialtyFromAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.SpecialtyId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
