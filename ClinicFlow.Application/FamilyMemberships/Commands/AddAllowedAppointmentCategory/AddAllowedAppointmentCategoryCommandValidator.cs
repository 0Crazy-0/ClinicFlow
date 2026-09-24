using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.FamilyMemberships.Commands.AddAllowedAppointmentCategory;

public sealed class AddAllowedAppointmentCategoryCommandValidator
    : AbstractValidator<AddAllowedAppointmentCategoryCommand>
{
    public AddAllowedAppointmentCategoryCommandValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.TargetUserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Category).IsInEnum().WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
