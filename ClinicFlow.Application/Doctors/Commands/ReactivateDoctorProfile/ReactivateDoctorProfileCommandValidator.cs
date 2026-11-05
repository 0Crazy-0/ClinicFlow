using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;

public sealed class ReactivateDoctorProfileCommandValidator
    : AbstractValidator<ReactivateDoctorProfileCommand>
{
    public ReactivateDoctorProfileCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
