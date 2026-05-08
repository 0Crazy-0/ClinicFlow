using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RestrictAppointmentTypeToSpecialties;

public class RestrictAppointmentTypeToSpecialtiesCommandValidator
    : AbstractValidator<RestrictAppointmentTypeToSpecialtiesCommand>
{
    public RestrictAppointmentTypeToSpecialtiesCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.SpecialtyIds).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleForEach(x => x.SpecialtyIds)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
