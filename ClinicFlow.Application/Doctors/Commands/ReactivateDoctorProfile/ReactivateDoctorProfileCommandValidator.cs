using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;

public class ReactivateDoctorProfileCommandValidator
    : AbstractValidator<ReactivateDoctorProfileCommand>
{
    public ReactivateDoctorProfileCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
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
