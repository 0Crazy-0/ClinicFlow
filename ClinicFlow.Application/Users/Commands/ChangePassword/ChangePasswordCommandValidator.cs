using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(8)
            .WithMessage(DomainErrors.Validation.ValueTooShort);
    }
}
