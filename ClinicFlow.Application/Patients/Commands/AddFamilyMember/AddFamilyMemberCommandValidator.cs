using ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;
using ClinicFlow.Domain.Common;
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
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
