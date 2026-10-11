using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Queries.GetMinorFamilyMemberAppointmentsByPatientId;

public sealed class GetMinorFamilyMemberAppointmentsByPatientIdQueryValidator
    : AbstractValidator<GetMinorFamilyMemberAppointmentsByPatientIdQuery>
{
    public GetMinorFamilyMemberAppointmentsByPatientIdQueryValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
