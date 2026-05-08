using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RemoveRequiredTemplateFromAppointmentType;

public sealed record RemoveRequiredTemplateFromAppointmentTypeCommand(
    Guid AppointmentTypeId,
    Guid TemplateId
) : IRequest;
