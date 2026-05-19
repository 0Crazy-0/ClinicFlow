using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Queries.CheckPhoneUniqueness;

public class CheckPhoneUniquenessQueryValidator : AbstractValidator<CheckPhoneUniquenessQuery>
{
    public CheckPhoneUniquenessQueryValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
