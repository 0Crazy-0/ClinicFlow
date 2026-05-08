using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.ChangeAppointmentTypeAgePolicy;

public sealed record ChangeAppointmentTypeAgePolicyCommand(
    Guid AppointmentTypeId,
    int? MinimumAge,
    int? MaximumAge,
    bool RequiresGuardianConsent
) : IRequest;
