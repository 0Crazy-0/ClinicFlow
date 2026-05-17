using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandValidator
    : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
