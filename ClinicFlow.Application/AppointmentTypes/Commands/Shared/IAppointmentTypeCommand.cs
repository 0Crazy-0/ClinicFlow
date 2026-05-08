using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.AppointmentTypes.Commands.Shared;

public interface IAppointmentTypeCommand
{
    AppointmentCategory Category { get; }
    string Name { get; }
    string Description { get; }
    TimeSpan DurationMinutes { get; }
}
