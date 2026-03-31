using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;

public class AddClinicalDetailToMedicalRecordCommandValidator
    : AbstractValidator<AddClinicalDetailToMedicalRecordCommand>
{
    public AddClinicalDetailToMedicalRecordCommandValidator()
    {
        RuleFor(x => x.MedicalRecordId).NotEmpty().WithMessage("Medical Record ID is required.");

        RuleFor(x => x.TemplateCode)
            .NotEmpty()
            .WithMessage("Template code is required for the clinical detail.");

        RuleFor(x => x.JsonDataPayload)
            .NotEmpty()
            .WithMessage("JSON data payload is required for the clinical detail.");
    }
}
