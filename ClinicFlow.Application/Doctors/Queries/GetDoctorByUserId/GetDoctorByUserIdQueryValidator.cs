using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorByUserId;

public class GetDoctorByUserIdQueryValidator : AbstractValidator<GetDoctorByUserIdQuery>
{
    public GetDoctorByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
