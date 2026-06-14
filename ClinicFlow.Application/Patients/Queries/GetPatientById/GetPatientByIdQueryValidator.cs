using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Queries.GetPatientById;

public sealed class GetPatientByIdQueryValidator : AbstractValidator<GetPatientByIdQuery>
{
    public GetPatientByIdQueryValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
