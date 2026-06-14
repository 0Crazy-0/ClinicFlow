using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Queries.GetLockedOutUsers;

public sealed class GetLockedOutUsersQueryValidator : AbstractValidator<GetLockedOutUsersQuery>
{
    public GetLockedOutUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
