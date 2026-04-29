using ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;
using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;

public abstract class CompletePatientCommandValidatorBase<TCommand>
    : CreatePatientCommandValidatorBase<TCommand>
    where TCommand : ICompletePatientCommand
{
    protected CompletePatientCommandValidatorBase()
    {
        RuleFor(x => x.BloodType).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EmergencyContactName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
