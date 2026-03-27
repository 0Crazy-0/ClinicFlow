namespace ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;

public interface IRescheduleCommand
{
    Guid InitiatorUserId { get; }
    Guid AppointmentId { get; }
    DateTime NewDate { get; }
    TimeSpan NewStartTime { get; }
    TimeSpan NewEndTime { get; }
}
