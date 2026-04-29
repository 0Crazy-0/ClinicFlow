using ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;
using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.AddFamilyMember;

public class AddFamilyMemberCommandValidator
    : CreatePatientCommandValidatorBase<AddFamilyMemberCommand>
{
    public AddFamilyMemberCommandValidator()
    {
        RuleFor(x => x.Relationship)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
