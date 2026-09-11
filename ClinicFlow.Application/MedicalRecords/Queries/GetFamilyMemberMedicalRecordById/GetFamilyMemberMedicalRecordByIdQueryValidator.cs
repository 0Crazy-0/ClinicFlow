using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetFamilyMemberMedicalRecordById;

public sealed class GetFamilyMemberMedicalRecordByIdQueryValidator
    : AbstractValidator<GetFamilyMemberMedicalRecordByIdQuery>
{
    public GetFamilyMemberMedicalRecordByIdQueryValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.MedicalRecordId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
