using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.CreateAppointmentType;

public sealed record CreateAppointmentTypeCommand(
    AppointmentCategory Category,
    string Name,
    string Description,
    TimeSpan DurationMinutes,
    int? MinimumAge,
    int? MaximumAge,
    bool RequiresGuardianConsent
) : IRequest<Guid>;
