using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetEligibleAppointmentTypes;

public sealed class GetEligibleAppointmentTypesQueryValidator
    : AbstractValidator<GetEligibleAppointmentTypesQuery>
{
    public GetEligibleAppointmentTypesQueryValidator()
    {
        RuleFor(x => x.PatientAgeInYears)
            .GreaterThanOrEqualTo(0)
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }
}
