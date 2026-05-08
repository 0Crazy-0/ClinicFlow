using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.MakeAppointmentTypeUnrestricted;

public sealed record MakeAppointmentTypeUnrestrictedCommand(Guid AppointmentTypeId) : IRequest;
