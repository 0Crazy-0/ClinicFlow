using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;

public sealed record ScheduleByPatientCommand(
    Guid InitiatorUserId,
    Guid TargetPatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? PatientNotes = null
) : IRequest<Guid>;
