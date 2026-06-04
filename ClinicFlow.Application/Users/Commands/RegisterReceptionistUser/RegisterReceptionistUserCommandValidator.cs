using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Commands.RegisterReceptionistUser;

public sealed class RegisterReceptionistUserCommandValidator
    : AbstractValidator<RegisterReceptionistUserCommand>
{
    public RegisterReceptionistUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(8)
            .WithMessage(DomainErrors.Validation.ValueTooShort);
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
