using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Schedules.Queries.GetSchedulesByDoctorId;

public class GetSchedulesByDoctorIdQueryValidator : AbstractValidator<GetSchedulesByDoctorIdQuery>
{
    public GetSchedulesByDoctorIdQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
