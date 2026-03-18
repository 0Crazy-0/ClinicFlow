using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;

public class UpdatePatientProfileCommandValidator : AbstractValidator<UpdatePatientProfileCommand>
{
    public UpdatePatientProfileCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.BloodType).NotEmpty();
        RuleFor(x => x.EmergencyContactName).NotEmpty();
        RuleFor(x => x.EmergencyContactPhone).NotEmpty();
    }
}
