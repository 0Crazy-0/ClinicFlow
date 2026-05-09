using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.DeactivateAppointmentType;

public sealed record DeactivateAppointmentTypeCommand(Guid AppointmentTypeId) : IRequest;
