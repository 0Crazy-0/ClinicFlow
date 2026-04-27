using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;

public class CreateDoctorProfileCommandValidator : AbstractValidator<CreateDoctorProfileCommand>
{
    public CreateDoctorProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.LicenseNumber).NotEmpty().MinimumLength(4);
        RuleFor(x => x.MedicalSpecialtyId).NotEmpty();
        RuleFor(x => x.ConsultationRoomNumber)
            .GreaterThan(0)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive)
            .LessThanOrEqualTo(35);
        RuleFor(x => x.ConsultationRoomName).NotEmpty();
        RuleFor(x => x.ConsultationRoomFloor)
            .GreaterThan(0)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive)
            .LessThanOrEqualTo(8);
    }
}
