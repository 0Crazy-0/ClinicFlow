using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.Users.Commands.LoginUser;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Email)
            .MaximumLength(EmailAddress.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.Password).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
