using ClinicFlow.Application.Schedules.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;

public sealed class GetScheduleByDoctorAndDayQueryHandler(IScheduleRepository scheduleRepository)
    : IRequestHandler<GetScheduleByDoctorAndDayQuery, ScheduleDto?>
{
    public async Task<ScheduleDto?> Handle(
        GetScheduleByDoctorAndDayQuery request,
        CancellationToken cancellationToken
    )
    {
        var schedule = await scheduleRepository.GetByDoctorAndDayAsync(
            request.DoctorId,
            request.DayOfWeek,
            cancellationToken
        );

        return schedule is null
            ? null
            : new ScheduleDto(
                schedule.Id,
                schedule.DoctorId,
                schedule.DayOfWeek,
                schedule.TimeRange.Start,
                schedule.TimeRange.End,
                schedule.IsActive
            );
    }
}
