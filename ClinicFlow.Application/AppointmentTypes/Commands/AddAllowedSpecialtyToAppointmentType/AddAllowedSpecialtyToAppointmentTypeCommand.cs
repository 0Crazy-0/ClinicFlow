using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.AddAllowedSpecialtyToAppointmentType;

public sealed record AddAllowedSpecialtyToAppointmentTypeCommand(
    Guid AppointmentTypeId,
    Guid SpecialtyId
) : IRequest;
