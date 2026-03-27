namespace ClinicFlow.Application.Appointments.Commands.Shared.Schedule;

public interface IScheduleCommand
{
    Guid InitiatorUserId { get; }
    Guid TargetPatientId { get; }
    Guid AppointmentTypeId { get; }
    DateTime ScheduledDate { get; }
    TimeSpan StartTime { get; }
    TimeSpan EndTime { get; }
}
