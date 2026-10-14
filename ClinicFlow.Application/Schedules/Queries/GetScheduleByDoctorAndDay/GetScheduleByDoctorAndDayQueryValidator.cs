using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;

public sealed class GetScheduleByDoctorAndDayQueryValidator
    : AbstractValidator<GetScheduleByDoctorAndDayQuery>
{
    public GetScheduleByDoctorAndDayQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Weekday).IsInEnum().WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
