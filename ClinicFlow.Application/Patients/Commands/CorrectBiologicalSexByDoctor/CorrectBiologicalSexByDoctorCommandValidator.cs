using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.CorrectBiologicalSexByDoctor;

public sealed class CorrectBiologicalSexByDoctorCommandValidator
    : AbstractValidator<CorrectBiologicalSexByDoctorCommand>
{
    public CorrectBiologicalSexByDoctorCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.CorrectedSex)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MaximumLength(Patient.MaxCorrectionReasonLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
