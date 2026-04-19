using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypeById;

public class GetAppointmentTypeByIdQueryValidator : AbstractValidator<GetAppointmentTypeByIdQuery>
{
    public GetAppointmentTypeByIdQueryValidator()
    {
        RuleFor(x => x.AppointmentTypeId).NotEmpty();
    }
}
