using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;

public class CreateDoctorProfileCommandValidator : AbstractValidator<CreateDoctorProfileCommand>
{
    public CreateDoctorProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.LicenseNumber)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(4)
            .WithMessage(DomainErrors.Validation.ValueTooShort);
        RuleFor(x => x.MedicalSpecialtyId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.ConsultationRoomNumber)
            .GreaterThan(0)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive)
            .LessThanOrEqualTo(35)
            .WithMessage(DomainErrors.Validation.ValueExceedsMaximum);
        RuleFor(x => x.ConsultationRoomName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.ConsultationRoomFloor)
            .GreaterThan(0)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive)
            .LessThanOrEqualTo(8)
            .WithMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }
}
