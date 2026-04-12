using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Commands.UpdateDoctorProfile;

public class UpdateDoctorProfileCommandValidator : AbstractValidator<UpdateDoctorProfileCommand>
{
    public UpdateDoctorProfileCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.ConsultationRoomNumber)
            .GreaterThan(0)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }
}
