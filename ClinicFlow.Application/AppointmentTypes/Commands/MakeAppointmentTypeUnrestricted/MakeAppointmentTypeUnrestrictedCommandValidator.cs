using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.MakeAppointmentTypeUnrestricted;

public sealed class MakeAppointmentTypeUnrestrictedCommandValidator
    : AbstractValidator<MakeAppointmentTypeUnrestrictedCommand>
{
    public MakeAppointmentTypeUnrestrictedCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
