using ClinicFlow.Application.Schedules.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;

public sealed class GetScheduleByDoctorAndDayQueryHandler(IScheduleRepository scheduleRepository)
    : IRequestHandler<GetScheduleByDoctorAndDayQuery, ScheduleDto?>
{
    /// <inheritdoc />
    public async Task<ScheduleDto?> Handle(
        GetScheduleByDoctorAndDayQuery request,
        CancellationToken cancellationToken
    )
    {
        var schedule = await scheduleRepository.GetActiveByDoctorAndDayAsync(
            request.DoctorId,
            request.Weekday,
            cancellationToken
        );

        return schedule is null
            ? null
            : new ScheduleDto(
                schedule.Id,
                schedule.DoctorId,
                schedule.Weekday,
                schedule.TimeSlot.Start,
                schedule.TimeSlot.End,
                schedule.IsActive
            );
    }
}
