using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.UpdateSchedule;

public sealed class UpdateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateScheduleCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        UpdateScheduleCommand request,
        CancellationToken cancellationToken
    )
    {
        var currentSchedule =
            await scheduleRepository.GetByDoctorAndDayAsync(
                request.DoctorId,
                request.DayOfWeek,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Schedule),
                request.DoctorId
            );

        currentSchedule.Deactivate();

        var newSchedule = Schedule.Create(
            request.DoctorId,
            request.DayOfWeek,
            TimeRange.Create(request.StartTime, request.EndTime)
        );

        await scheduleRepository.CreateAsync(newSchedule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return newSchedule.Id;
    }
}
