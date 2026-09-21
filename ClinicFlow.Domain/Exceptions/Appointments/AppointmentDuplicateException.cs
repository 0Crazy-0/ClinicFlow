using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentDuplicateException(string errorCode, Guid patientId, Guid appointmentTypeId)
    : DomainException(errorCode)
{
    public Guid PatientId { get; } = patientId;
    public Guid AppointmentTypeId { get; } = appointmentTypeId;
}
