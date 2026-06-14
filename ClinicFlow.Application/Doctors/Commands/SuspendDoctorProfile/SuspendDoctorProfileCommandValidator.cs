using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Doctors.Commands.SuspendDoctorProfile;

public sealed class SuspendDoctorProfileCommandValidator
    : AbstractValidator<SuspendDoctorProfileCommand>
{
    public SuspendDoctorProfileCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
