using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;

public class GetScheduleByDoctorAndDayQueryValidator
    : AbstractValidator<GetScheduleByDoctorAndDayQuery>
{
    public GetScheduleByDoctorAndDayQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.DayOfWeek).IsInEnum().WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
