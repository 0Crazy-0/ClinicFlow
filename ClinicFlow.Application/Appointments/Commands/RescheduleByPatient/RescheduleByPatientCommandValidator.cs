using ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;
using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;

public class RescheduleByPatientCommandValidator
    : RescheduleCommandValidatorBase<RescheduleByPatientCommand>
{
    public RescheduleByPatientCommandValidator(TimeProvider timeProvider)
        : base(timeProvider)
    {
        RuleFor(x => x.NewPatientNotes)
            .MaximumLength(500)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
