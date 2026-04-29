using ClinicFlow.Application.Appointments.Commands.Shared.Schedule;
using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;

public class ScheduleByPatientCommandValidator
    : ScheduleCommandValidatorBase<ScheduleByPatientCommand>
{
    public ScheduleByPatientCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
