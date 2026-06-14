using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Queries.GetPatientsByUserId;

public sealed class GetPatientsByUserIdQueryValidator : AbstractValidator<GetPatientsByUserIdQuery>
{
    public GetPatientsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
