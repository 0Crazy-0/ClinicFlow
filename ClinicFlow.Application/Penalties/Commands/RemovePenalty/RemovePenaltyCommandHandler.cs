using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Penalties.Commands.RemovePenalty;

public sealed class RemovePenaltyCommandHandler(
    IPatientPenaltyRepository penaltyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RemovePenaltyCommand>
{
    public async Task Handle(RemovePenaltyCommand request, CancellationToken cancellationToken)
    {
        var penalty =
            await penaltyRepository.GetByIdAsync(request.PenaltyId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(PatientPenalty),
                request.PenaltyId
            );

        penalty.Remove();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
