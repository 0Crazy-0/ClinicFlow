using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.AddAllowedSpecialtyToAppointmentType;

public class AddAllowedSpecialtyToAppointmentTypeCommandValidator
    : AbstractValidator<AddAllowedSpecialtyToAppointmentTypeCommand>
{
    public AddAllowedSpecialtyToAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.SpecialtyId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
