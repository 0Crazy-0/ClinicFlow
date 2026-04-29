using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;

public class GetDoctorsBySpecialtyIdQueryValidator : AbstractValidator<GetDoctorsBySpecialtyIdQuery>
{
    public GetDoctorsBySpecialtyIdQueryValidator()
    {
        RuleFor(x => x.SpecialtyId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
