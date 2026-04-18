using FluentValidation;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryValidator
    : AbstractValidator<GetAppointmentsByDoctorIdQuery>
{
    public GetAppointmentsByDoctorIdQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
    }
}
