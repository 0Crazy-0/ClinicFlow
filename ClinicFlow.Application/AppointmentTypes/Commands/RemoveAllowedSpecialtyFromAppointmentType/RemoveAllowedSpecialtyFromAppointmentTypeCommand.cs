using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RemoveAllowedSpecialtyFromAppointmentType;

public sealed record RemoveAllowedSpecialtyFromAppointmentTypeCommand(
    Guid AppointmentTypeId,
    Guid SpecialtyId
) : IRequest;
