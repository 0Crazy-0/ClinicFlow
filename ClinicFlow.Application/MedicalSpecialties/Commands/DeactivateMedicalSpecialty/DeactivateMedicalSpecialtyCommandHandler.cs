using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.DeactivateMedicalSpecialty;

public sealed class DeactivateMedicalSpecialtyCommandHandler(
    IMedicalSpecialtyRepository medicalSpecialtyRepository,
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeactivateMedicalSpecialtyCommand>
{
    public async Task Handle(
        DeactivateMedicalSpecialtyCommand request,
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

        var hasActiveDoctors = await doctorRepository.HasActiveBySpecialtyIdAsync(
            specialty.Id,
            cancellationToken
        );

        specialty.Deactivate(hasActiveDoctors);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
