using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;

public class CompleteMedicalEncounterCommandValidator
    : AbstractValidator<CompleteMedicalEncounterCommand>
{
    public CompleteMedicalEncounterCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);

        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);

        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);

        RuleFor(x => x.ChiefComplaint)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);

        RuleForEach(x => x.Details)
            .ChildRules(details =>
            {
                details
                    .RuleFor(d => d.TemplateCode)
                    .NotEmpty()
                    .WithMessage(DomainErrors.Validation.ValueRequired);
                details
                    .RuleFor(d => d.JsonDataPayload)
                    .NotEmpty()
                    .WithMessage(DomainErrors.Validation.ValueRequired);
            });
    }
}
