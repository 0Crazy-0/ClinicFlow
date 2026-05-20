using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.CreateMedicalSpecialty;

public sealed class CreateMedicalSpecialtyCommandHandler(
    IMedicalSpecialtyRepository medicalSpecialtyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateMedicalSpecialtyCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        CreateMedicalSpecialtyCommand request,
        CancellationToken cancellationToken
    )
    {
        if (await medicalSpecialtyRepository.ExistsByNameAsync(request.Name, cancellationToken))
            throw new BusinessRuleValidationException(
                DomainErrors.MedicalSpecialty.NameAlreadyExists
            );

        var specialty = MedicalSpecialty.Create(
            request.Name,
            request.Description,
            request.TypicalDurationMinutes,
            request.MinCancellationHours
        );

        await medicalSpecialtyRepository.CreateAsync(specialty, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return specialty.Id;
    }
}
