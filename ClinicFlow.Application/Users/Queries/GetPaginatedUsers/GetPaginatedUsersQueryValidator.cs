using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Users.Queries.GetPaginatedUsers;

public class GetPaginatedUsersQueryValidator : AbstractValidator<GetPaginatedUsersQuery>
{
    public GetPaginatedUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
