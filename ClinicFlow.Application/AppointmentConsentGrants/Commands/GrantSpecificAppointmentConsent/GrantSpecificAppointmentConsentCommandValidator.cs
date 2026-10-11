using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantSpecificAppointmentConsent;

public sealed class GrantSpecificAppointmentConsentCommandValidator
    : AbstractValidator<GrantSpecificAppointmentConsentCommand>
{
    public GrantSpecificAppointmentConsentCommandValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.RecipientMembershipId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
