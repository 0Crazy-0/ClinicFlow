using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;

public class GetMedicalRecordByAppointmentIdQueryValidator
    : AbstractValidator<GetMedicalRecordByAppointmentIdQuery>
{
    public GetMedicalRecordByAppointmentIdQueryValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}
