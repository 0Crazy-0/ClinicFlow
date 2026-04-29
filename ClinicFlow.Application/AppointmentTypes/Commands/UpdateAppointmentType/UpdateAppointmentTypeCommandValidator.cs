using ClinicFlow.Application.AppointmentTypes.Commands.Shared;
using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;

public class UpdateAppointmentTypeCommandValidator
    : AppointmentTypeCommandValidatorBase<UpdateAppointmentTypeCommand>
{
    public UpdateAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
