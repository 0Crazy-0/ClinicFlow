using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.CreateSchedule;

public sealed class CreateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateScheduleCommand, Guid>
{
    public async Task<Guid> Handle(CreateScheduleCommand request, CancellationToken ct)
    {
        var existingSchedules = await scheduleRepository.GetByDoctorIdAsync(request.DoctorId, ct);

        Schedule.EnsureNoDuplicateDay(existingSchedules, request.DoctorId, request.DayOfWeek);

        var schedule = Schedule.Create(
            request.DoctorId,
            request.DayOfWeek,
            TimeRange.Create(request.StartTime, request.EndTime)
        );

        await scheduleRepository.CreateAsync(schedule, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return schedule.Id;
    }
}
