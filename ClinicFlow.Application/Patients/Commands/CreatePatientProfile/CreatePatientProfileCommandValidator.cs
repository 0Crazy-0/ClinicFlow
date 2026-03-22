using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandValidator : AbstractValidator<CreatePatientProfileCommand>
{
    public CreatePatientProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.LastName).NotEmpty().MinimumLength(2);
        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }
}
