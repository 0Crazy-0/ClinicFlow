using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.AppointmentTypes.Commands.Shared;

public interface IAppointmentTypeCommand
{
    AppointmentCategory Category { get; }
    string Name { get; }
    string Description { get; }
    TimeSpan DurationMinutes { get; }
    int? MinimumAge { get; }
    int? MaximumAge { get; }
    bool RequiresGuardianConsent { get; }
}
