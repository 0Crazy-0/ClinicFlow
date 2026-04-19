using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByCategory;

public class GetAppointmentTypesByCategoryQueryValidator
    : AbstractValidator<GetAppointmentTypesByCategoryQuery>
{
    public GetAppointmentTypesByCategoryQueryValidator()
    {
        RuleFor(x => x.Category).IsInEnum();
    }
}
