using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;

public class CreateDoctorProfileCommandValidator : AbstractValidator<CreateDoctorProfileCommand>
{
    public CreateDoctorProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(PersonName.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(PersonName.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(PersonName.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(PersonName.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.LicenseNumber)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(MedicalLicenseNumber.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(MedicalLicenseNumber.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.MedicalSpecialtyId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.ConsultationRoomNumber)
            .GreaterThanOrEqualTo(ConsultationRoom.MinimumNumber)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive)
            .LessThanOrEqualTo(ConsultationRoom.MaximumNumber)
            .WithMessage(DomainErrors.Validation.ValueExceedsMaximum);
        RuleFor(x => x.ConsultationRoomName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.ConsultationRoomFloor)
            .GreaterThanOrEqualTo(ConsultationRoom.MinimumFloor)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive)
            .LessThanOrEqualTo(ConsultationRoom.MaximumFloor)
            .WithMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }
}
