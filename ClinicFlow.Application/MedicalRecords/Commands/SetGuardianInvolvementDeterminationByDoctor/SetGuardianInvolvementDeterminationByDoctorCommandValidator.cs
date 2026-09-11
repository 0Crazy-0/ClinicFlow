using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Commands.SetGuardianInvolvementDeterminationByDoctor;

public sealed class SetGuardianInvolvementDeterminationByDoctorCommandValidator
    : AbstractValidator<SetGuardianInvolvementDeterminationByDoctorCommand>
{
    public SetGuardianInvolvementDeterminationByDoctorCommandValidator()
    {
        RuleFor(x => x.MedicalRecordId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
