using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Queries.CheckEmailUniqueness;

public class CheckEmailUniquenessQueryValidator : AbstractValidator<CheckEmailUniquenessQuery>
{
    public CheckEmailUniquenessQueryValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
