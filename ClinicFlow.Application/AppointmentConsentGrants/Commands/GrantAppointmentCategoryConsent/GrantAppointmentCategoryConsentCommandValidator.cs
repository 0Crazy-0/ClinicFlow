using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantAppointmentCategoryConsent;

public sealed class GrantAppointmentCategoryConsentCommandValidator
    : AbstractValidator<GrantAppointmentCategoryConsentCommand>
{
    public GrantAppointmentCategoryConsentCommandValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.RecipientMembershipId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.ProtectedCareCategory)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
