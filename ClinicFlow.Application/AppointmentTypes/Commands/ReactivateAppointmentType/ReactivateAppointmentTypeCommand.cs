using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.ReactivateAppointmentType;

public sealed record ReactivateAppointmentTypeCommand(Guid AppointmentTypeId) : IRequest;
