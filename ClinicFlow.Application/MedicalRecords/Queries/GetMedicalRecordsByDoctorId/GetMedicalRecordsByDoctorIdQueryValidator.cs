using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public class GetMedicalRecordsByDoctorIdQueryValidator
    : AbstractValidator<GetMedicalRecordsByDoctorIdQuery>
{
    public GetMedicalRecordsByDoctorIdQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
