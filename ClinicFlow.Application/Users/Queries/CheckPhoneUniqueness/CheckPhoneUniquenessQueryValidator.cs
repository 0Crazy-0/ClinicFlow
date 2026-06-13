using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.Users.Queries.CheckPhoneUniqueness;

public class CheckPhoneUniquenessQueryValidator : AbstractValidator<CheckPhoneUniquenessQuery>
{
    public CheckPhoneUniquenessQueryValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MaximumLength(PhoneNumber.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
