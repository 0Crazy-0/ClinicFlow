using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;

public sealed record UpdateAppointmentTypeCommand(
    Guid AppointmentTypeId,
    AppointmentCategory Category,
    string Name,
    string Description,
    TimeSpan DurationMinutes,
    int? MinimumAge,
    int? MaximumAge,
    bool RequiresGuardianConsent
) : IRequest;
