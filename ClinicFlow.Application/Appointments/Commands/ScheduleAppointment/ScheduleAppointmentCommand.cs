using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleAppointment;

public record ScheduleAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime
) : IRequest<Guid>;
