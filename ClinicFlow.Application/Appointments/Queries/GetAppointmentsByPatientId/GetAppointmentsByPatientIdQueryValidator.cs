using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdQueryValidator
    : AbstractValidator<GetAppointmentsByPatientIdQuery>
{
    public GetAppointmentsByPatientIdQueryValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
