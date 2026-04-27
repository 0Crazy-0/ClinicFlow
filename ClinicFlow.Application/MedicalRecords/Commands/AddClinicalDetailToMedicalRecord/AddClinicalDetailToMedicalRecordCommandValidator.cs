using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;

public class AddClinicalDetailToMedicalRecordCommandValidator
    : AbstractValidator<AddClinicalDetailToMedicalRecordCommand>
{
    public AddClinicalDetailToMedicalRecordCommandValidator()
    {
        RuleFor(x => x.MedicalRecordId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);

        RuleFor(x => x.TemplateCode).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);

        RuleFor(x => x.JsonDataPayload)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
