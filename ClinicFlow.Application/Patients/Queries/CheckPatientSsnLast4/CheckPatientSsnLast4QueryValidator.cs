using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Queries.CheckPatientSsnLast4;

public sealed class CheckPatientSsnLast4QueryValidator
    : AbstractValidator<CheckPatientSsnLast4Query>
{
    public CheckPatientSsnLast4QueryValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Last4)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue)
            .Matches(@"^[0-9]{4}\z")
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
