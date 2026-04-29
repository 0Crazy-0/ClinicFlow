using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;

public class UpdatePatientProfileCommandValidator : AbstractValidator<UpdatePatientProfileCommand>
{
    public UpdatePatientProfileCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.BloodType).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EmergencyContactName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
