using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Commands.SendPhoneVerification;

public sealed class SendPhoneVerificationCommandValidator
    : AbstractValidator<SendPhoneVerificationCommand>
{
    public SendPhoneVerificationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
