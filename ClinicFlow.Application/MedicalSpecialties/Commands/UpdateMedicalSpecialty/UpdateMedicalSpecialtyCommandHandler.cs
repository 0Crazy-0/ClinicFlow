using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.UpdateMedicalSpecialty;

public sealed class UpdateMedicalSpecialtyCommandHandler(
    IMedicalSpecialtyRepository medicalSpecialtyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateMedicalSpecialtyCommand>
{
    public async Task Handle(
        UpdateMedicalSpecialtyCommand request,
        CancellationToken cancellationToken
    )
    {
        var specialty =
            await medicalSpecialtyRepository.GetByIdAsync(request.SpecialtyId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalSpecialty),
                request.SpecialtyId
            );

        specialty.UpdateDetails(
            request.Name,
            request.Description,
            request.TypicalDurationMinutes,
            request.MinCancellationHours
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
