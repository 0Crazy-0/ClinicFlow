using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.ReactivateMedicalSpecialty;

public sealed class ReactivateMedicalSpecialtyCommandHandler(
    IMedicalSpecialtyRepository medicalSpecialtyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReactivateMedicalSpecialtyCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        ReactivateMedicalSpecialtyCommand request,
        CancellationToken cancellationToken
    )
    {
        var specialty =
            await medicalSpecialtyRepository.GetByIdIncludingDeletedAsync(
                request.SpecialtyId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalSpecialty),
                request.SpecialtyId
            );

        specialty.Reactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
