using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecordConsentGrants.Queries.GetMedicalRecordConsentGrantsByPatientId;

public sealed class GetMedicalRecordConsentGrantsByPatientIdQueryValidator
    : AbstractValidator<GetMedicalRecordConsentGrantsByPatientIdQuery>
{
    public GetMedicalRecordConsentGrantsByPatientIdQueryValidator()
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
