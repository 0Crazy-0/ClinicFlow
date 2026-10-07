using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecordConsentGrants.Commands.RevokeMedicalRecordConsentGrant;

public sealed class RevokeMedicalRecordConsentGrantCommandValidator
    : AbstractValidator<RevokeMedicalRecordConsentGrantCommand>
{
    public RevokeMedicalRecordConsentGrantCommandValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.ConsentGrantId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
