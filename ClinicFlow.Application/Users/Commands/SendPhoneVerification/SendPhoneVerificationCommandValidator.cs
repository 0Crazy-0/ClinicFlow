using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Commands.SendPhoneVerification;

public class SendPhoneVerificationCommandValidator : AbstractValidator<SendPhoneVerificationCommand>
{
    public SendPhoneVerificationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
