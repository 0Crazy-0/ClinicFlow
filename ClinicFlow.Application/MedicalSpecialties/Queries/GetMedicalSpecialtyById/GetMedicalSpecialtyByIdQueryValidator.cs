using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalSpecialties.Queries.GetMedicalSpecialtyById;

public class GetMedicalSpecialtyByIdQueryValidator : AbstractValidator<GetMedicalSpecialtyByIdQuery>
{
    public GetMedicalSpecialtyByIdQueryValidator()
    {
        RuleFor(x => x.MedicalSpecialtyId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
