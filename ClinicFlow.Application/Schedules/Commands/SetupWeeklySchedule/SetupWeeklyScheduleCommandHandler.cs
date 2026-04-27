using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Schedule;
using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;

public sealed class SetupWeeklyScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetupWeeklyScheduleCommand, IReadOnlyList<Guid>>
{
    public async Task<IReadOnlyList<Guid>> Handle(
        SetupWeeklyScheduleCommand request,
        CancellationToken cancellationToken
    )
    {
        var existingSchedules = await scheduleRepository.GetByDoctorIdAsync(
            request.DoctorId,
            cancellationToken
        );

        var slots = request
            .Slots.Select(s => new WeeklyScheduleSlot(s.DayOfWeek, s.StartTime, s.EndTime))
            .ToList();

        var schedules = WeeklyScheduleSetupService
            .SetupWeeklySchedule(request.DoctorId, existingSchedules, slots)
            .ToList();

        await scheduleRepository.CreateRangeAsync(schedules, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return [.. schedules.Select(s => s.Id)];
    }
}
