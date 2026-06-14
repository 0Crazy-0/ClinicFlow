using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;

public sealed class GetDoctorsBySpecialtyIdQueryValidator
    : AbstractValidator<GetDoctorsBySpecialtyIdQuery>
{
    public GetDoctorsBySpecialtyIdQueryValidator()
    {
        RuleFor(x => x.SpecialtyId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
