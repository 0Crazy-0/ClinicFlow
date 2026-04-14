using ClinicFlow.Application.Schedules.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleById;

public sealed class GetScheduleByIdQueryHandler(IScheduleRepository scheduleRepository)
    : IRequestHandler<GetScheduleByIdQuery, ScheduleDto>
{
    public async Task<ScheduleDto> Handle(
        GetScheduleByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var schedule =
            await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Schedule),
                request.ScheduleId
            );

        return new ScheduleDto(
            schedule.Id,
            schedule.DoctorId,
            schedule.DayOfWeek,
            schedule.TimeRange.Start,
            schedule.TimeRange.End,
            schedule.IsActive
        );
    }
}
