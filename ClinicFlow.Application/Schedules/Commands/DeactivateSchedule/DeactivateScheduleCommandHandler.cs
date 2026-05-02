using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.DeactivateSchedule;

public sealed class DeactivateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeactivateScheduleCommand>
{
    public async Task Handle(DeactivateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule =
            await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Schedule),
                request.ScheduleId
            );

        schedule.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
