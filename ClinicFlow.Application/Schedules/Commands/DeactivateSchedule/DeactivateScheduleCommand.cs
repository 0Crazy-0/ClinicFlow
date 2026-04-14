using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.DeactivateSchedule;

public sealed record DeactivateScheduleCommand(Guid ScheduleId) : IRequest;
