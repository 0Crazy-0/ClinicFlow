using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorIdAndDate;

public sealed class GetAppointmentsByDoctorIdAndDateQueryValidator
    : AbstractValidator<GetAppointmentsByDoctorIdAndDateQuery>
{
    public GetAppointmentsByDoctorIdAndDateQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Date).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
