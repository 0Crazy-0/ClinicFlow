using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.FamilyMemberships.Commands.RemoveAllowedAppointmentCategory;

public sealed class RemoveAllowedAppointmentCategoryCommandValidator
    : AbstractValidator<RemoveAllowedAppointmentCategoryCommand>
{
    public RemoveAllowedAppointmentCategoryCommandValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.TargetUserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Category).IsInEnum().WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
