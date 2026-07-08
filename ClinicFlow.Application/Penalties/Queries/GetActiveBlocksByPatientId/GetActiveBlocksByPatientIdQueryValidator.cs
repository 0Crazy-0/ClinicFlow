using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveBlocksByPatientId;

public sealed class GetActiveBlocksByPatientIdQueryValidator
    : AbstractValidator<GetActiveBlocksByPatientIdQuery>
{
    public GetActiveBlocksByPatientIdQueryValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
