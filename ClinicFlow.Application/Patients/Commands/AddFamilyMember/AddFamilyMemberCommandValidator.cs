using ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.AddFamilyMember;

public class AddFamilyMemberCommandValidator
    : CreatePatientCommandValidatorBase<AddFamilyMemberCommand>
{
    public AddFamilyMemberCommandValidator(TimeProvider timeProvider)
        : base(timeProvider)
    {
        RuleFor(x => x.Relationship)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue)
            .DependentRules(() =>
            {
                RuleFor(x => x.Relationship)
                    .NotEqual(PatientRelationship.Self)
                    .WithMessage(DomainErrors.Patient.CannotBeSelf);
            });
    }
}
