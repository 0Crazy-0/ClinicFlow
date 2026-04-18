using FluentValidation;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryValidator : AbstractValidator<GetAppointmentByIdQuery>
{
    public GetAppointmentByIdQueryValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}
