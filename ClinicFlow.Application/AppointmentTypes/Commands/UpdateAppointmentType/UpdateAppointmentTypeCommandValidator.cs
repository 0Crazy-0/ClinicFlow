using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;

public sealed class UpdateAppointmentTypeCommandValidator
    : AbstractValidator<UpdateAppointmentTypeCommand>
{
    public UpdateAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Name).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.DurationMinutes)
            .Must(EncounterDuration.IsValid)
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
    }
}
