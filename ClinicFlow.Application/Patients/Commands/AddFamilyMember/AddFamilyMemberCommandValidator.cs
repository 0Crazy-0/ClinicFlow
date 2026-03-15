using FluentValidation;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Application.Patients.Commands.AddFamilyMember;

public class AddFamilyMemberCommandValidator : AbstractValidator<AddFamilyMemberCommand>
{
    public AddFamilyMemberCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.DateOfBirth).LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
        RuleFor(x => x.BloodType).NotEmpty();
        RuleFor(x => x.EmergencyContactName).NotEmpty();
        RuleFor(x => x.EmergencyContactPhone).NotEmpty();
        RuleFor(x => x.Relationship).IsInEnum();
    }
}
