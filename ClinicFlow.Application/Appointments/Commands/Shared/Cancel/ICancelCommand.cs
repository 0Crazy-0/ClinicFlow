namespace ClinicFlow.Application.Appointments.Commands.Shared.Cancel;

public interface ICancelCommand
{
    Guid AppointmentId { get; }
    Guid InitiatorUserId { get; }
    string? Reason { get; }
}
