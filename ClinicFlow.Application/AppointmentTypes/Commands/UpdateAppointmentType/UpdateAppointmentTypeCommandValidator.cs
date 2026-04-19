using ClinicFlow.Application.AppointmentTypes.Commands.Shared;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;

public class UpdateAppointmentTypeCommandValidator
    : AppointmentTypeCommandValidatorBase<UpdateAppointmentTypeCommand>
{
    public UpdateAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId).NotEmpty();
    }
}
