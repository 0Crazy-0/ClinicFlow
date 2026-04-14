using ClinicFlow.Application.Schedules.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Schedules.Queries.GetSchedulesByDoctorId;

public sealed record GetSchedulesByDoctorIdQuery(Guid DoctorId)
    : IRequest<IReadOnlyList<ScheduleDto>>;
