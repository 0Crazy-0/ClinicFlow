using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;

public class CompleteMedicalEncounterCommandValidator
    : AbstractValidator<CompleteMedicalEncounterCommand>
{
    public CompleteMedicalEncounterCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("Doctor ID is required.");

        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");

        RuleFor(x => x.ChiefComplaint).NotEmpty().WithMessage("Chief complaint is required.");

        RuleForEach(x => x.Details)
            .ChildRules(details =>
            {
                details
                    .RuleFor(d => d.TemplateCode)
                    .NotEmpty()
                    .WithMessage("Template code is required for each clinical detail.");
                details
                    .RuleFor(d => d.JsonDataPayload)
                    .NotEmpty()
                    .WithMessage("JSON data payload is required for each clinical detail.");
            });
    }
}
