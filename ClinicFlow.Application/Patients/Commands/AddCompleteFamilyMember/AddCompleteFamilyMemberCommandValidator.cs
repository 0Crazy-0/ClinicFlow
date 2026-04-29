using ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;
using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;

public class AddCompleteFamilyMemberCommandValidator
    : CompletePatientCommandValidatorBase<AddCompleteFamilyMemberCommand>
{
    public AddCompleteFamilyMemberCommandValidator()
    {
        RuleFor(x => x.Relationship)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
