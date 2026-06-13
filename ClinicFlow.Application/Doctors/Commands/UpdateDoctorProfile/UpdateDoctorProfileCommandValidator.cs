using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Commands.UpdateDoctorProfile;

public class UpdateDoctorProfileCommandValidator : AbstractValidator<UpdateDoctorProfileCommand>
{
    public UpdateDoctorProfileCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
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
