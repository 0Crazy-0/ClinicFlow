using ClinicFlow.Application.Schedules.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Schedules.Queries.GetSchedulesByDoctorId;

public sealed class GetSchedulesByDoctorIdQueryHandler(IScheduleRepository scheduleRepository)
    : IRequestHandler<GetSchedulesByDoctorIdQuery, IReadOnlyList<ScheduleDto>>
{
    public async Task<IReadOnlyList<ScheduleDto>> Handle(
        GetSchedulesByDoctorIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var schedules = await scheduleRepository.GetByDoctorIdAsync(
            request.DoctorId,
            cancellationToken
        );

        return
        [
            .. schedules.Select(schedule => new ScheduleDto(
                schedule.Id,
                schedule.DoctorId,
                schedule.DayOfWeek,
                schedule.TimeRange.Start,
                schedule.TimeRange.End,
                schedule.IsActive
            )),
        ];
    }
}
