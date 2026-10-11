using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.RevokeAppointmentConsentGrant;

public sealed class RevokeAppointmentConsentGrantCommandValidator
    : AbstractValidator<RevokeAppointmentConsentGrantCommand>
{
    public RevokeAppointmentConsentGrantCommandValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.ConsentGrantId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
