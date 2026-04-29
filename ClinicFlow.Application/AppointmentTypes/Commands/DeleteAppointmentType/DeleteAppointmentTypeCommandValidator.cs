using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.DeleteAppointmentType;

public class DeleteAppointmentTypeCommandValidator : AbstractValidator<DeleteAppointmentTypeCommand>
{
    public DeleteAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
