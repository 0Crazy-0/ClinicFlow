using ClinicFlow.Application.Schedules.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;

public sealed record GetScheduleByDoctorAndDayQuery(Guid DoctorId, DayOfWeek DayOfWeek)
    : IRequest<ScheduleDto?>;
