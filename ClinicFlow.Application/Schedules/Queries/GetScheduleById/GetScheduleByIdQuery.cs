using ClinicFlow.Application.Schedules.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleById;

public sealed record GetScheduleByIdQuery(Guid ScheduleId) : IRequest<ScheduleDto>;
