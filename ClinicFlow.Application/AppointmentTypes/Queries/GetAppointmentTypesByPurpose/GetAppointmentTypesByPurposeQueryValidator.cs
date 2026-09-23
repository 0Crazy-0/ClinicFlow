using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByPurpose;

public sealed class GetAppointmentTypesByPurposeQueryValidator
    : AbstractValidator<GetAppointmentTypesByPurposeQuery>
{
    public GetAppointmentTypesByPurposeQueryValidator()
    {
        RuleFor(x => x.Purpose).IsInEnum().WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
