using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.AddRequiredTemplateToAppointmentType;

public sealed record AddRequiredTemplateToAppointmentTypeCommand(
    Guid AppointmentTypeId,
    Guid TemplateId
) : IRequest;
