using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.DeleteAppointmentType;

public sealed record DeleteAppointmentTypeCommand(Guid AppointmentTypeId) : IRequest;
