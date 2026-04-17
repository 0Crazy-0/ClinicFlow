using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Commands.BlockPatient;

public sealed class BlockPatientCommandHandler(
    TimeProvider timeProvider,
    IPatientPenaltyRepository penaltyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<BlockPatientCommand, Guid>
{
    public async Task<Guid> Handle(BlockPatientCommand request, CancellationToken cancellationToken)
    {
        var penalty = PatientPenalty.CreateManualBlock(
            request.PatientId,
            request.Reason,
            request.Duration,
            timeProvider.GetUtcNow().UtcDateTime
        );

        await penaltyRepository.AddAsync(penalty, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return penalty.Id;
    }
}
