using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryValidator
    : AbstractValidator<GetAppointmentsByDoctorIdQuery>
{
    public GetAppointmentsByDoctorIdQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Date).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
