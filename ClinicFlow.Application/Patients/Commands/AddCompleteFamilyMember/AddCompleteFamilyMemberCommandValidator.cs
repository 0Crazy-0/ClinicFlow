using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;

public class AddCompleteFamilyMemberCommandValidator
    : AbstractValidator<AddCompleteFamilyMemberCommand>
{
    public AddCompleteFamilyMemberCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
        RuleFor(x => x.BloodType).NotEmpty();
        RuleFor(x => x.EmergencyContactName).NotEmpty();
        RuleFor(x => x.EmergencyContactPhone).NotEmpty();
        RuleFor(x => x.Relationship).IsInEnum();
    }
}
